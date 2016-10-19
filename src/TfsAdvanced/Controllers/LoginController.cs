using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.ServiceRequests;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using TfsAdvanced.Data;
using TfsAdvanced.Infrastructure;


namespace TfsAdvanced.Controllers
{
    [Route("data/Login")]
    public class LoginController : Controller
    {
        private readonly AuthorizationRequest authorizationRequest;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly CacheStats cacheStats;

        public LoginController(AuthorizationRequest authorizationRequest, SignInManager<ApplicationUser> signInManager, CacheStats cacheStats)
        {
            this.authorizationRequest = authorizationRequest;
            this.signInManager = signInManager;
            this.cacheStats = cacheStats;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string ReturnURL = null)
        {
            //return Redirect(authorizationRequest.GetChallengeUrl(GetBaseURL()));
            return Redirect(authorizationRequest.GetADChallengeUrl(GetBaseURL()));
        }

        [HttpGet("LoginAuth")]
        [AllowAnonymous]
        public async Task<IActionResult> ADLogin(string code = null, string state = null, bool Admin_consent = false, string Session_state = null)
        {
            var token = await authorizationRequest.GetADAccessToken(GetBaseURL(), code, state);

            this.
            HttpContext.Session.Set("AuthToken", ASCIIEncoding.ASCII.GetBytes(JsonConvert.SerializeObject(token)));

            cacheStats.UserLogin();

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

            cacheStats.UserLogin();

            var securityToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(token.access_token);

            
            return Ok(token);

            return Redirect("/data/PullRequests");
        }

        private string GetBaseURL()
        {
            return(HttpContext.Request.IsHttps ? "https://" : "http://") + HttpContext.Request.Host;
        }
    }
}
