// ---------------------------------------------------------------------------------
// <copyright company="Microsoft">
//   Copyright (c) 2017 Microsoft Corporation. All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.Authentication
{
    using AzureMapsB2C;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using System;
    using System.Collections.Generic;
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
                options.MetadataAddress = _azureOptions.MetadataAddress;
                options.UseTokenLifetime = true;
                options.CallbackPath = _azureOptions.CallbackPath;
                options.RequireHttpsMetadata = false;
                options.ClientSecret = _azureOptions.ClientSecret;

                // Without overriding the response type (which by default is id_token), the OnAuthorizationCodeReceived event is not called.
                // but instead OnTokenValidated event is called. Here we request both so that OnTokenValidated is called first which 
                // ensures that context.Principal has a non-null value when OnAuthorizeationCodeReceived is called
                options.ResponseType = "code id_token";

                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("offline_access");
                options.Scope.Add($"{_azureOptions.Resource}/user_impersonation");

                // Subscribing to the OIDC events
                options.Events.OnAuthorizationCodeReceived = OnAuthorizationCodeReceived;
                options.Events.OnAuthenticationFailed = OnAuthenticationFailed;
                options.Events.OnMessageReceived = OnMessageReceived;
            }

            private Task OnMessageReceived(MessageReceivedContext arg)
            {
                return Task.CompletedTask;
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

                var clientApp = (IConfidentialClientApplication)context.HttpContext.RequestServices.GetService(typeof(IConfidentialClientApplication));
                
                List<string> scopes = new List<string>()
                {
                    "offline_access",
                    $"{_azureOptions.Resource}/user_impersonation"
                };

                var authResult = await clientApp
                    .AcquireTokenByAuthorizationCode(scopes, context.TokenEndpointRequest.Code)
                    .ExecuteAsync();

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
