using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.Web.Controllers
{
    [Route("/WebSocketClient")]
    public class WebSocketClientController : Controller
    {
        private readonly WebSocketClientRepository webSocketClientRepository;

        public WebSocketClientController(WebSocketClientRepository webSocketClientRepository)
        {
            this.webSocketClientRepository = webSocketClientRepository;
        }

        [HttpGet]
        public IActionResult GetClients()
        {
            var clients = webSocketClientRepository.GetClients();
            return Ok(clients);
        }
    }
}
