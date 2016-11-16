using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.Repository;

namespace TfsAdvanced.Controllers
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
                {"LoadedPercent", hangFireStatusRepository.PercentLoaded()}
            });
        }
    }
}
