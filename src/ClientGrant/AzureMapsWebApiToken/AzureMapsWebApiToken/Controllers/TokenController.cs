using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Services.AppAuthentication;

namespace AzureMapsWebApiToken.Controllers
{
    /// <summary>
    /// In general, an "open" controller like this is not considered production ready. 
    /// We recommend using <see cref="AuthorizeAttribute"/> and applying Authenication on your application.
    /// Our recommendated solution is Azure Active Directory but UserName-Password implementations work too.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        /// <summary>
        /// This token provider simplifies access tokens for Azure Resources. It uses the Managed Identity of the deployed resource.
        /// For instance if this application was deployed to Azure App Service or Azure Virtual Machine, you can assign an Azure AD
        /// identity and this library will use that identity when deployed to production.
        /// </summary>
        /// <remarks>
        /// For the Web SDK to authorize correctly, you still must assign Azure role based access control for the managed identity
        /// as explained in the readme.md. There is significant benefit which is outlined in the the readme.
        /// </remarks>
        private static readonly AzureServiceTokenProvider tokenProvider = new AzureServiceTokenProvider();

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetTokenAsync()
        {
            // tokenProvider will cache the token in memory, if you would like to reduce the dependency on Azure AD we recommend
            // implementing a distributed cache combined with using the other methods available on tokenProvider.
            string accessToken = await tokenProvider.GetAccessTokenAsync("https://atlas.microsoft.com/", cancellationToken: HttpContext.RequestAborted);
            
            return Ok(accessToken);
        }
    }
}
