using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.Repository;

namespace TfsAdvanced.Controllers
{
    [Route("data/Pools")]
    public class PoolsController : Controller
    {
        private readonly PoolRepository poolRepository;

        public PoolsController(PoolRepository poolRepository)
        {
            this.poolRepository = poolRepository;
        }

        [HttpGet]
        public IActionResult GetPools()
        {
            return Ok(poolRepository.GetPools());
        }
    }
}
