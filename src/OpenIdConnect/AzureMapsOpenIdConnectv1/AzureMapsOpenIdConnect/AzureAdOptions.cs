// ---------------------------------------------------------------------------------
// <copyright company="Microsoft">
//   Copyright (c) 2017 Microsoft Corporation. All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.Authentication
{
    using Microsoft.Azure.KeyVault;
    using System.Threading.Tasks;

    public class AzureAdOptions
    {
        private readonly IKeyVaultClient keyVaultClient;

        public AzureAdOptions(IKeyVaultClient keyVaultClient)
        {
            this.keyVaultClient = keyVaultClient;
        }

        /// <summary>
        /// The Azure AD Resource for Azure Maps
        /// </summary>
        public string Resource { get; } = "https://atlas.microsoft.com/";

        /// <summary>
        /// ClientId (Application Id) of this Web Application
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Client Secret (Application password) added in the Azure portal in the Keys section for the application
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Azure AD Cloud instance
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        ///  domain of your tenant, e.g. contoso.onmicrosoft.com
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Tenant Id, as obtained from the Azure portal:
        /// (Select 'Endpoints' from the 'App registrations' blade and use the GUID in any of the URLs)
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// URL on which this Web App will be called back by Azure AD (normally "/signin-oidc")
        /// </summary>
        public string CallbackPath { get; set; }

        /// <summary>
        /// Authority delivering the token for your tenant
        /// </summary>
        public string Authority
        {
            get
            {
                return $"{Instance}{TenantId}";
            }
        }

        /// <summary>
        /// Simple opinionated setting load, Microsoft recommends using Azure Key Vault to store any application secrets.
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            Instance = "https://login.microsoftonline.com/";
            CallbackPath = "/signin-oidc";
            ClientId = "cf637c1c-1d5f-477d-92c6-228a11d3402b";
            TenantId = "0994ea7b-72bb-41d3-a599-62c5fb189234";
            Domain = "azuremaps.onmicrosoft.com";

            // in production, we strongly suggest to observe the pressure given on the key vault and consider caching the application secret.
            // it will be necessary to provision a secret and authenticate to Azure AD to complete the authorization code grant flow.
            // We recommend provisioning a key vault and using Managed Service Identity to authenticate to this Key Vault.
            var secret = await keyVaultClient.GetSecretAsync("https://skymap.vault.azure.net/secrets/SkymapAppPassword/");
            ClientSecret = secret.Value;
        }
    }
}
