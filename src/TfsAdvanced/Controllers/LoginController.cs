using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.ServiceRequests;
using Microsoft.AspNetCore.Identity;
using TfsAdvanced.Data;

namespace TfsAdvanced.Controllers
{
    [Route("data/Login")]
    public class LoginController : Controller
    {
        private readonly AuthorizationRequest authorizationRequest;
        private readonly SignInManager<User> signInManager;

        public LoginController(AuthorizationRequest authorizationRequest, SignInManager<User> signInManager)
        {
            this.authorizationRequest = authorizationRequest;
            this.signInManager = signInManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string ReturnURL = null)
        {
            var redirectURL = Url.Action("Callback", "Login");
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Microsoft", redirectURL);
            return Challenge(properties, "Microsoft");
        }

        [HttpGet("Callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback(string returnUrl = null, string remoteError = null)
        {
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                return Redirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return BadRequest("Account is locked out.");
            }

            return Ok("User does not have an account.");
        }
    }
}
