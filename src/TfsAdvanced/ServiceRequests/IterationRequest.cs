using TfsAdvanced.Data;
using TfsAdvanced.Infrastructure;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TfsAdvanced.ServiceRequests
{
    public class IterationRequest
    {
        private readonly AppSettings appSettings;

        public IterationRequest(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public async Task<Iteration> CreateIteration(RequestData requestData, Iteration iteration)
        {
            var url = $"{requestData.BaseAddress}/{iteration.project.name}/_apis/wit/classificationNodes/iterations?api-version=1.0";
            HttpContent content = new StringContent(JsonConvert.SerializeObject(new CreateIteration
            {
                name = iteration.name,
                attributes = iteration.attributes
            }), Encoding.UTF8, "application/json");
            var response = await requestData.HttpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Iteration>(responseContent);

            return new Iteration
            {
                name = responseContent
            };
        }

        public async Task<Iteration> GetIteration(RequestData requestData, Project project)
        {
            var response = await requestData.HttpClient.GetStringAsync($"{requestData.BaseAddress}/{project.id}/_apis/wit/classificationNodes/iterations?api-version=1.0&$depth=1");
            var iteration = JsonConvert.DeserializeObject<Iteration>(response);
            iteration.project = project;
            SetProject(iteration, project);

            return iteration;
        }

        private void SetProject(Iteration iteration, Project project)
        {
            iteration.project = project;
            if (iteration.hasChildren)
                iteration.children.ForEach(c => SetProject(c, project));
        }
    }
}