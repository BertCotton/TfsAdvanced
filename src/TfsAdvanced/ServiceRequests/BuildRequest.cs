using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.Infrastructure;

namespace TfsAdvanced.ServiceRequests
{
    public class BuildRequest
    {
        private readonly AppSettings appSettings;

        public BuildRequest(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public IList<Build> GetAllBuilds(RequestData requestData)
        {
            var builds = new List<Build>();
            appSettings.Projects.ForEach(project =>
            {
                builds.AddRange(GetBuilds(requestData, project).Result);
            });
            return builds;
        }

        public async Task<IList<Build>> GetBuilds(RequestData requestData, string tfsProject)
        {
            var response = await
                requestData.HttpClient.GetStringAsync($"{requestData.BaseAddress}/{tfsProject}/_apis/build/builds?api-version=2.2&minFinishTime={DateTime.Now.Date.AddDays(-1).ToString("o")}");
            var responseObject = JsonConvert.DeserializeObject<Response<IEnumerable<Build>>>(response);

            var builds = responseObject.value.ToList();

            builds.ForEach(build => build.buildUrl = $"{requestData.BaseAddress}/{tfsProject}/_build?_a=summary&buildId={build.id}");

            return responseObject.value.ToList();
        }
    }
}