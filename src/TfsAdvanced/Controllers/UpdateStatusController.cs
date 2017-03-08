using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.DataStore.Repository;

namespace TfsAdvanced.Web.Controllers
{
    [Route("data/UpdateStatus")]
    public class UpdateStatusController : Controller
    {
        private readonly UpdateStatusRepository updateStatusRepository;

        public UpdateStatusController(UpdateStatusRepository updateStatusRepository)
        {
            this.updateStatusRepository = updateStatusRepository;
        }

        [HttpGet]
        public IActionResult GetUpdateStatuses()
        {
            return Ok(updateStatusRepository.GetStatuses());
        }
    }
}
