---
page_type: sample
languages:
- csharp
- javascript
products:
- azure-maps
- aspnetcore
- azure-ad
description: "A collection of samples showing how to integrate Azure Active Directory with Azure Maps."
urlFragment: "AzureMapsAADSamples"
---

# Official Microsoft Sample

<!-- 
Guidelines on README format: https://review.docs.microsoft.com/help/onboard/admin/samples/concepts/readme-template?branch=master

Guidance on onboarding samples to docs.microsoft.com/samples: https://review.docs.microsoft.com/help/onboard/admin/samples/process/onboarding?branch=master

Taxonomies for products and languages: https://review.docs.microsoft.com/new-hope/information-architecture/metadata/taxonomies?branch=master
-->

These are 4 different samples using AspNetCore C# to quick start Azure AD authentication to Azure Maps. Each sample uses different authentication protocols depending on application need which are supported by Azure AD and Azure Role Based Access Control (RBAC).

## Contents

Outline the file contents of the repository. It helps users navigate the codebase, build configuration and any related assets.

| File/folder               | Description                                                                  |
|---------------------------|------------------------------------------------------------------------------|
| `src/ImplicitGrant`       | Samples used to show user authentication without a server component.         |
| `src/OpenIdConnect`       | Samples using Microsoft's recommended protocol for secure web applications   |
| `src/ClientGrant`         | Samples showing application authentication without user interaction.         |
| `.gitignore`              | Define what to ignore at commit time.                                        |
| `CONTRIBUTING.md`         | Guidelines for contributing to the sample.                                   |
| `README.md`               | The starting readme.                                                         |
| `LICENSE`                 | The license for the sample.                                                  |

## Prerequisites

Prior to downloading these samples

- [Visual Studio 2019 or Visual Studio Code](https://visualstudio.microsoft.com/downloads/?utm_content=download+vs2019) with **ASP.NET and web development** workload.
- You will need an Azure Subscription, sign up for a [free account](https://azure.microsoft.com/en-us/free/search/) if necessary.
- a **free SKU** for [Azure Active Directory](https://azure.microsoft.com/en-us/trial/get-started-active-directory/) associated with the Azure Subscription.

## Setup

### In the Azure Active Directory, create new application registration via [Azure Portal](https://portal.azure.com/)

- This application registration will represent the web application(s).
- Each specific sample will describe the steps necessary for the different authentication protocols.
- For the sake of this sample repository, the **same application registration** can be used.
- For production we recommend a distinct application registration for each web application. Additionally, we highly recommend using [Azure Managed Identity](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview) for any non-interactive autentication to Azure Maps. This will save credential management costs.
- For display name, we can name it "WebApp" and leave the redirect uri empty for now and follow the individuals samples `README.md`.

### In the Azure Portal [create an Azure Maps account](https://portal.azure.com/#create/Microsoft.Maps)

- Search for "Azure Maps" on create new resource and follow the portal to create a new account.
- Once the account is created, retrieve the Azure Maps Client ID and keep on hand for the specific sample you wish to
run.
- This value should be used in the x-ms-client-id with all HTTP requests.
- If using any SDK add it to the authenication options (JS).

## Runnning the sample

- Running the Web Applicaton samples can be found based on the [AspNetCore MVC v2.2 documentation](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/start-mvc?view=aspnetcore-2.2&tabs=visual-studio)
- Once all the individual sample README.md have been configured, Debug (F5 key) should build and start the application.
- Make sure to run the web application with HTTPS configured.
- If prompted for development HTTPS certificate via IIS or AspNetCore, accept the prompt to trust the certificate.

## Key concepts

- Using Azure Maps Web SDK supports 2 approaches for Azure AD access tokens.
- If a server component like AspNetCore MVC is available for your application, we recommend [OpenID Connect](https://azure.microsoft.com/en-us/resources/samples/active-directory-dotnet-webapp-openidconnect-aspnetcore/).
- In the case of no server component, you must use implicit grant for an user interactive sign in experience. However,
  in the case for no interactive sign in, some server component must exist to retrieve an access token and provide it
  to the Azure Maps Web SDK.
- Using Azure Service Authentication Library will help reduce the complexity and cost of credential management and allow for [Azure Managed Identities](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview) to be used on the hosted platform such as Azure Virtual Machines or Azure App Service.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
