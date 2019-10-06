// ---------------------------------------------------------------------------------
// <copyright company="Microsoft">
//   Copyright (c) 2017 Microsoft Corporation. All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.Authentication
{
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// An opinionated implementation of adding OpenIdConnect to the AspNetCore middleware, this is simply to quickstart.
    /// </summary>
    public static class AzureAdAuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder)
            => builder.AddAzureAd(_ => { });

        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder, Action<AzureAdOptions> configureOptions)
        {            
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureAzureOptions>();
            builder.AddOpenIdConnect();
            return builder;
        }

        private class ConfigureAzureOptions : IConfigureNamedOptions<OpenIdConnectOptions>
        {
            private readonly AzureAdOptions _azureOptions;

            public ConfigureAzureOptions(AzureAdOptions azureOptions)
            {
                _azureOptions = azureOptions;
            }

            public void Configure(string name, OpenIdConnectOptions options)
            {
                _azureOptions.InitializeAsync().Wait();
                options.ClientId = _azureOptions.ClientId;
                options.Authority = _azureOptions.Authority;
                options.UseTokenLifetime = true;
                options.CallbackPath = _azureOptions.CallbackPath;
                options.RequireHttpsMetadata = false;
                options.ClientSecret = _azureOptions.ClientSecret;
                options.Resource = _azureOptions.Resource; // Azure Maps

                // Without overriding the response type (which by default is id_token), the OnAuthorizationCodeReceived event is not called.
                // but instead OnTokenValidated event is called. Here we request both so that OnTokenValidated is called first which 
                // ensures that context.Principal has a non-null value when OnAuthorizeationCodeReceived is called
                options.ResponseType = "id_token code";

                // Subscribing to the OIDC events
                options.Events.OnAuthorizationCodeReceived = OnAuthorizationCodeReceived;
                options.Events.OnAuthenticationFailed = OnAuthenticationFailed;
            }

            public void Configure(OpenIdConnectOptions options)
            {
                Configure(Options.DefaultName, options);
            }

            /// <summary>
            /// Redeems the authorization code by calling AcquireTokenByAuthorizationCodeAsync in order to ensure
            /// that the cache has a token for the signed-in user, which will then enable the controllers (like the
            /// TodoController, to call AcquireTokenSilentAsync successfully.
            /// </summary>
            private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
            {
                // refresh app secret
                await _azureOptions.InitializeAsync();

                // at this point you can augment the claim's principal if you know they belong to a special admin group or
                // have different privledges.
                string userObjectId = context.Principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                
                // retrieve the distributed cache and cache the access token by the user's object id.
                // for the local sample, a memory cache will be used and the session will not persist.
                IDistributedCache cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
                var mapsCache = new NaiveSessionCache(userObjectId, cache);
                var mapsAuthContext = new AuthenticationContext(context.Options.Authority, mapsCache);

                var credential = new ClientCredential(context.Options.ClientId, context.Options.ClientSecret);
                var authResult = await mapsAuthContext.AcquireTokenByAuthorizationCodeAsync(context.TokenEndpointRequest.Code,
                    new Uri(context.TokenEndpointRequest.RedirectUri, UriKind.RelativeOrAbsolute), credential,
                    context.Options.Resource);
                
                // Notify the OIDC middleware that we already took care of code redemption.
                context.HandleCodeRedemption(authResult.AccessToken, context.ProtocolMessage.IdToken);
            }

            /// <summary>
            /// this method is invoked if exceptions are thrown during request processing
            /// </summary>
            private Task OnAuthenticationFailed(AuthenticationFailedContext context)
            {
                context.HandleResponse();
                context.Response.Redirect("/Home/Error?message=" + context.Exception.Message);
                return Task.FromResult(0);
            }
        }
    }
}
