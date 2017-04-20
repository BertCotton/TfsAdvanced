using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.DataStore.Repository;

namespace TfsAdvanced.Web.Controllers
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
