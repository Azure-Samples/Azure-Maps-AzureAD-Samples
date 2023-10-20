using Azure.Identity;
using Azure.Core;
using System.Net.Http.Headers;

/// <summary>
/// This token provider simplifies access tokens for Azure Resources. It uses Managed Identity of the the deployed resource.
/// For instance if this application was deployed to Azure App Service or Azure Virtual Machine, you can assign an Azure AD
/// identity and this library will use that identity when deployed to production.
/// </summary>
/// <remarks>
/// For the REST API to authorize correctly, you still must assign Azure role based access control for the managed identity
/// as explained in the readme.md. There is significant benefit which is outlined in the the readme.
/// </remarks>

// Azure Map API URL
const string AzureMapsApiUrl = "https://atlas.microsoft.com/search/poi/json?api-version=1.0&query=statue%20of%20liberty";

// Azure Map API client ID
const string AzureMapsClientId = "11111111-2222-3333-4444-555555555555";


Console.WriteLine("Starting...");

using var httpClient = new HttpClient();

// Create an instance of DefaultAzureCredential
var defaultAzureCredential = new DefaultAzureCredential();

// Request a token from Azure Identity
var tokenRequestContext = new TokenRequestContext(new[] { "https://atlas.microsoft.com/.default" });
var accessToken = await defaultAzureCredential.GetTokenAsync(tokenRequestContext);

using var requestMessage = new HttpRequestMessage(HttpMethod.Get, AzureMapsApiUrl);

// Set the access token in the request headers
requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

// Set the Azure Maps client ID
requestMessage.Headers.Add("x-ms-client-id", AzureMapsClientId);

using var responseMessage = await httpClient.SendAsync(requestMessage);

if (responseMessage.IsSuccessStatusCode)
{
    Console.WriteLine("Response was success!");
    string responseBody = await responseMessage.Content.ReadAsStringAsync();
    Console.WriteLine($"Response Body: {responseBody}");
}
else
{
    Console.WriteLine($"Response Failed with Status Code: {responseMessage.StatusCode}");
    Console.WriteLine($"Response Headers: {responseMessage.Headers}");
    string responseBody = await responseMessage.Content.ReadAsStringAsync();
    Console.WriteLine($"Response Body: {responseBody}");
}