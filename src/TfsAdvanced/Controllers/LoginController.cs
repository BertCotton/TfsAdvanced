using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TfsAdvanced.Models.Infrastructure;
using TfsAdvanced.ServiceRequests;

namespace TfsAdvanced.Web.Controllers
{
    [Route("data/Login")]
    public class LoginController : Controller
    {
        private readonly AuthorizationRequest authorizationRequest;

        public LoginController(AuthorizationRequest authorizationRequest)
        {
            this.authorizationRequest = authorizationRequest;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string ReturnURL = null)
        {
            return Redirect(authorizationRequest.GetADChallengeUrl(GetBaseURL()));
        }

        [HttpGet("LoginAuth")]
        [AllowAnonymous]
        public async Task<IActionResult> ADLogin(string code = null, string state = null, bool Admin_consent = false, string Session_state = null)
        {
            var token = await authorizationRequest.GetADAccessToken(GetBaseURL(), code, state);

            this.
            HttpContext.Session.Set("AuthToken", ASCIIEncoding.ASCII.GetBytes(JsonConvert.SerializeObject(token)));
            
            return Redirect("/");
        }

        [HttpGet("LoginVSOAuth")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAuth(string code = null, string state = null, bool Admin_consent = false, string Session_state = null)
        {
            var tokenString = await authorizationRequest.GetVSOAccessToken(GetBaseURL(), code, state);

            var token = JsonConvert.DeserializeObject<AuthenticationToken>(tokenString);

            if (String.IsNullOrEmpty(token.access_token))
                throw new Exception("The access token is null");

            var cookieValue = JsonConvert.SerializeObject(token);
            HttpContext.Session.Set("AuthToken", ASCIIEncoding.ASCII.GetBytes(JsonConvert.SerializeObject(token)));
            HttpContext.Response.Cookies.Append("Auth", cookieValue, new CookieOptions
            {
                Secure = true,
                Expires = DateTime.Now.AddYears(1),
                HttpOnly = true,
                Path = "/",
                Domain = HttpContext.Request.Host.ToString()
            });
            
            return Redirect("/");
        }

        private string GetBaseURL()
        {
            return "https://" + HttpContext.Request.Host;
        }
    }
}
