using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.DataStore.Repository;

namespace TfsAdvanced.Web.Controllers
{
    [Route("Health")]
    public class HealthController : Controller
    {
        private readonly HangFireStatusRepository hangFireStatusRepository;

        public HealthController(HangFireStatusRepository hangFireStatusRepository)
        {
            this.hangFireStatusRepository = hangFireStatusRepository;
        }

        [HttpGet("LoadedStatus")]
        public IActionResult IsLoaded()
        {
            return Ok(new Dictionary<string, object>
            {
                {"IsLoaded", hangFireStatusRepository.IsLoaded()},
                {"LoadedPercent", Math.Round(hangFireStatusRepository.PercentLoaded(), 2)}
            });
        }
    }
}
