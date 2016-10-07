using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.ServiceRequests;

namespace TfsAdvanced.Controllers
{
    [Authorize]
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
        public IList<Build> Index()
        {
            return buildRequest.GetAllBuilds(requestData);
        }
    }
}