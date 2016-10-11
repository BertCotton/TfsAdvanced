using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.ServiceRequests;

namespace TfsAdvanced.Controllers
{
    [Route("data/Builds")]
    public class BuildsController : Controller
    {
        private readonly BuildRequest buildRequest;
        private readonly RequestData requestData;

        public BuildsController(RequestData requestData, BuildRequest buildRequest)
        {
            this.requestData = requestData;
            this.buildRequest = buildRequest;
        }

        [HttpGet]
        public async Task<IList<Build>> Index()
        {
            return await buildRequest.GetAllBuilds(requestData);
        }
    }
}