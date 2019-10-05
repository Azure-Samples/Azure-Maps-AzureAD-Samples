using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
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
        private readonly IDistributedCache cache;

        public TokenController(AzureAdOptions options, IDistributedCache cache)
        {
            this.options = options;
            this.cache = cache;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetTokenAsync()
        {
            // the object id is Azure AD's unique identifier for the authenticated user.
            string objectId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            return new ContentResult()
            {
                Content = await AcquireAccessTokenAsync(objectId, options.Resource),
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        /// <summary>
        /// This will acquire an access token for Azure Maps silently. This may fail of the session is not cached.
        /// </summary>
        private async Task<string> AcquireAccessTokenAsync(string objectId, string resource)
        {
            AuthenticationContext authenticationContext = new AuthenticationContext(
                options.Authority,
                new NaiveSessionCache(objectId, cache));
            ClientCredential credential = new ClientCredential(options.ClientId, options.ClientSecret);

            // this is what makes OpenID Connect special. Since we have maintained a backchannel authenticated session
            // we can acquire an access token for Azure Maps used the refresh token without a need for another sign-in.
            var result = await authenticationContext.AcquireTokenSilentAsync(resource, credential,
                new UserIdentifier(objectId, UserIdentifierType.UniqueId));

            return result.AccessToken;

        }
    }
}