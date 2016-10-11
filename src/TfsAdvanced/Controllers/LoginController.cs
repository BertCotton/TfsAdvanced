using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
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
            return Redirect(authorizationRequest.GetChallengeUrl((HttpContext.Request.IsHttps ? "https://" :"http://") + HttpContext.Request.Host.ToString()));
        }

        [HttpGet("LoginAuth")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAuth(string code = null, string state = null)
        {
            var token = await authorizationRequest.GetAccessToken(code, state);
            
            HttpContext.Response.Cookies.Append("Auth", JsonConvert.SerializeObject(token), new CookieOptions
            {
                Secure = true,
                Expires = DateTime.Now.AddYears(1),
                HttpOnly = true
            });

            cacheStats.UserLogin();

            return Redirect("/");
        }
    }
}
