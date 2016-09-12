using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
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
        public IList<Build> Index()
        {
            return buildRequest.GetAllBuilds(requestData);
        }
    }
}