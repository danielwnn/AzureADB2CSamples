using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OidcWeb.Controllers
{
    public class AccountController : Controller
    {

        [HttpGet]
        [AllowAnonymous]
        [Route("Account/SignIn/{scheme?}")]
        public IActionResult SignIn(string scheme)
        {
            scheme = this.HandleScheme(scheme);
            return this.Challenge(new AuthenticationProperties { RedirectUri = "/" }, scheme);
        }

        [HttpGet]
        [Route("Account/SignOut/{scheme?}")]
        public IActionResult SignOut(string scheme)
        {
            scheme = this.HandleScheme(scheme);

            // We will also specify the cooke authentication scheme when signing out, because we
            // have specified that as default scheme in Startup.cs.
            return this.SignOut(new AuthenticationProperties { RedirectUri = "/" }, scheme, CookieAuthenticationDefaults.AuthenticationScheme);
        }


        /// <summary>
        /// Returns the default challenge scheme if the given scheme is not specified.
        /// </summary>
        private string HandleScheme(string scheme)
        {
            return scheme ?? OpenIdConnectDefaults.AuthenticationScheme;
        }

    }
}