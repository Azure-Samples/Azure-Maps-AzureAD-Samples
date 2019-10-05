using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AzureMapsAzureAdDaemon
{
    class Program
    {
        /// <summary>
        /// This token provider simplifies access tokens for Azure Resources. It uses Managed Identity of the the deployed resource.
        /// For instance if this application was deployed to Azure App Service or Azure Virtual Machine, you can assign an Azure AD
        /// identity and this library will use that identity when deployed to production.
        /// </summary>
        /// <remarks>
        /// For the REST API to authorize correctly, you still must assign Azure role based access control for the managed identity
        /// as explained in the readme.md. There is significant benefit which is outlined in the the readme.
        /// </remarks>
        private static readonly AzureServiceTokenProvider tokenProvider = new AzureServiceTokenProvider();

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            using (HttpClient httpClient = new HttpClient())
            {
                // https://docs.microsoft.com/en-us/rest/api/maps/search/getsearchpoi
                string url = "https://atlas.microsoft.com/search/poi/json?api-version=1.0&query=statue of liberty";
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                // read more about the security definition
                // https://docs.microsoft.com/en-us/rest/api/maps/search/getsearchpoi#azure-auth
                string accessToken = tokenProvider.GetAccessTokenAsync("https://atlas.microsoft.com/").Result;
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // replace value with your x-ms-client-id found on the Azure Map account.
                requestMessage.Headers.Add("x-ms-client-id", "bde2322c-4f3e-494d-bc31-fa68db51d5f4");

                HttpResponseMessage responseMessage = httpClient.SendAsync(requestMessage).Result;

                if (responseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine("Response was success!");
                    Console.WriteLine("Response Body: {0}", responseMessage.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    Console.WriteLine("Response Failed with Status Code: {0}", responseMessage.StatusCode);
                    Console.WriteLine("Response Headers: {0}", responseMessage.Headers);
                    Console.WriteLine("Response Body: {0}", responseMessage.Content.ReadAsStringAsync().Result);
                }
            }
        }
    }
}
