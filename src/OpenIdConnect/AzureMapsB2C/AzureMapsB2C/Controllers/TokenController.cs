using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureMapsWebApiToken.Controllers
{
    /// <summary>
    /// Adding <see cref="AuthorizeAttribute"/> will ensure that only authenticated users on the web application can access this resource.
    /// </summary>
    /// <remarks>
    /// Someone may ask, "If we add this, why do we need an Additional token?"
    /// The additional access token enables authentication and authorization for that specific individual to Azure Maps.
    /// Sensitive can be restricted to the correct individuals this way.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TokenController : ControllerBase
    {
        private readonly AzureAdOptions options;
        private readonly IConfidentialClientApplication clientApplication;

        public TokenController(AzureAdOptions options, IConfidentialClientApplication clientApplication)
        {
            this.options = options;
            this.clientApplication = clientApplication;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetTokenAsync()
        {
            // the object id is Azure AD's unique identifier for the authenticated user.
            return new ContentResult()
            {
                Content = await AcquireAccessTokenAsync(),
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        /// <summary>
        /// This will acquire an access token for Azure Maps silently. This may fail of the session is not cached.
        /// </summary>
        private async Task<string> AcquireAccessTokenAsync()
        {
            string tenantId = options.TenantId;
            string objectId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            string policy = User.FindFirst("tfp").Value.ToLower();

            // msal provides an id format like:
            string id = $"{objectId}-{policy}.{tenantId}";
            var account = await clientApplication.GetAccountAsync(id);

            var result = await clientApplication
                .AcquireTokenSilent(new string[] { $"{options.Resource}/user_impersonation" }, account)
                .ExecuteAsync();

            return result.AccessToken;

        }
    }
}