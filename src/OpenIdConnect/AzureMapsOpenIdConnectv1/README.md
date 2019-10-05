# Sample Aspnet Core Web App with Azure Maps and Azure AD

This sample shows a DHTML (Razor Views) with a server-driven user login experience. This Azure AD authentication flow is referred to as [OpenID Connect](https://docs.microsoft.com/en-us/azure/active-directory/develop/v1-protocols-openid-connect-code). OpenID Connect is suitable for when a server component is available for authentication, it presents a benefit of single sign on using the session to continue authenication. If you try running this sample Azure AD will challenge saying you do not belong to the configured Azure AD tenant / directory. You **must** configure your own tenant and application; then update the configuration.

## Steps to configure

1. Create a new Web App Asp.Net Core from Visual Studio template.
2. Create new application registration in [Azure Portal](https://portal.azure.com/) for Azure AD directory.
3. Create another application registration in Azure Portal representing the development environment.
4. Create Azure Key Vault and store application secret.
5. Create an Azure Maps Account resource.
6. Assign users, groups, or applications to Azure Maps Access Management.

### Details

#### Creating new web application

Why: we need to host the web application.

1. When using Azure AD, it is **critical** to secure your application with an SSL certificate. Visual Studio tools provide AspNetCore applications with a developer SSL certificate which you must trust when prompted during development / debugging of the application.
2. We've added a partial view _LoginPartial.cshtml for Sign-in and Sign-out.
3. The in AspNetCore 2.2, microsoft.aspnetcore.authentication.openidconnect Nuget package simplifies the boilerplate code necessary hook up the flow. However we have added some additional opinionated support.
   1. Adding AzureAdOptions.cs will enable the settings and secrets necessary to acquire an access token for Azure Maps
   2. The AzureAdAuthenticationBuilderExtensions.cs orchestrates this necessary flow on the OpenID Connect middleware.
   3. Startup.cs has been configured to register the necessary dependencies and middleware configuration.
   4. We've addded an Azure Active Directory Authentication Library Token Cache implementation called NaiveSessionCache.cs which will cache any refresh token based on the authenticated user.
   5. The TokenController.cs is responsible for returning the access token to the Azure Maps Web SDK. This controller requires an authenticated user which restricts the usage of the SDK. Additional authorization can be configured by assigning users and groups to the Azure Maps account access control.
   6. For simplicity, we've created a new page called Maps.cshtml and added it to the Layout.cshtml to host the Web SDK. This web page will require authentication to navigate to it.

#### Creating new Application Registration in Azure AD for the Web App

Why: we must model and represent the application in Azure AD.

1. Azure AD has documentation on [how to register an application](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app). The only option which is currently well documented and supported is "Accounts in this organizational directory only".
   - Supporting multi-tenant users and applications is outside scope but we recommend troublshooting on [access control for guest users](https://docs.microsoft.com/en-us/azure/role-based-access-control/role-assignments-external-users#troubleshoot).
  
2. In this specific sample, we must add a redirect uri for `http://localhost:5001/Maps` because we will invoke the authentication sign in from that page.
3. Now the application should be granted access to Azure Maps. In 'API Permissions', add 'Azure Maps' with the 'user_impersonation' scope under [delegated permission](https://docs.microsoft.com/en-us/azure/active-directory/develop/v1-permissions-and-consent).
4. Next step is to create a secret to authenticate to Azure AD, this can be secret string. This secret will be necessary for the application to request an access token on behalf of the user once the authorization code has been redeemed.
5. You will need this secret to add to the Key Vault you will need to create.

#### Create an Application Registration in Azure AD for your Development Environment

Why: Since this sample requires runtime secrets, the application must be granted access to Azure Key Vault. This is an opinionated step, configuration and setup to access Azure Key Vault using [Azure Service Authentication Library C#](https://docs.microsoft.com/en-us/azure/key-vault/service-to-service-authentication). This can can be implemented in other ways, but our recommendation is keep credentials out of code and source control.

1. Create application registration similar to above but name it specific for Development Environment. You do not need to provide a redirect Uri.
2. Create a secret string, capture the [application (client) id and the tenant](https://docs.microsoft.com/en-us/azure/active-directory/develop/app-registrations-training-guide#new-ui). The application id should be a `guid` and it represents the app in Azure AD. The tenant should be in the form of `mytenant.onmicrosoft.com`. With all these values you can create a connection string `RunAs=App;AppId=[application id];TenantId=[mytenant.onmicrosoft.com];AppKey=[secret string]`
3. Create an Windows / System environment variable named `AzureServicesAuthConnectionString` with the connection string created above.

#### Create an Azure Key Vault Resource

Why: To store the secret required for the application to acquire an access token once the authorization code has been redeemed. We will use a Key Vault to securely store the string and add the access policy for development environment to securely access the key vault.

1. There are a few ways to create an Azure Key Vault, the easiest is likely going to the [Azure Portal](https://portal.azure.com/) and searching for Key Vault resource and following the UI. However for production based implmentation we suggest you create an Azure Resource Management template to provide a repeatable deployment.
2. Once the Key Vault has been created, we must take the secret from the Web Application and [set the secret in the Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/quick-create-portal).
3. Take the secret id from the portal and update AzureAdOptions.cs for InitializeAsync()
4. Now we must added the Development Environment principal to access the secret, go to the "access policies" blade, and add an access policy with secret permission "Get". Select principal, and search for the display name of your Development Environment application.
5. Save the policy, this will grant the application access to read the secret.

#### Create an Azure Maps Account Resource

Why: we need Azure Maps account resource to authorize with.

1. Refer to the Azure Maps Resource created in the Parent `README.md`.
2. Once the resource has been created, go to the authentication blade and copy the [client id property](https://docs.microsoft.com/en-us/azure/azure-maps/how-to-manage-authentication#view-authentication-details). This `guid` should be used in the `Maps.cshtml` page to indicate the Map account to be used by your application.

#### Assigning Users or Groups to the Azure Maps Resource

Why: we need to grant the right people or groups of people access to the Azure Maps account. These people should already exist in the Azure AD tenant and be added to the group which you specified from Azure AD. Read more about Azure AD groups [here](https://docs.microsoft.com/en-us/azure/active-directory/fundamentals/active-directory-manage-groups).

1. On the Azure Portal with the previously created Azure Maps resource. Navigate to the access control (IAM) blade. On this UI, you can add a role assignment. This will grant the user access to a pre-defined role of permssions for Azure Maps.
2. Search for the user or group by name or email and select them.
3. Selet Azure Maps Data Reader role definition, currently we support this role but if more fine grain permissions are required, please reach for Azure Maps feedback.
4. Save the role assignment.
   - The order of creating this role assignment was defined because we assigned on the Azure Maps Resource. However a higher scope of access can be created at the Resource Group or Subsubscription. Please read [Role Based Access Control](https://docs.microsoft.com/en-us/azure/role-based-access-control/role-definitions#data-operations-example) to futher meet your needs.
   - Tip: you can use, [Azure Resource Manager Template](https://docs.microsoft.com/en-us/azure/role-based-access-control/role-assignments-template) to also assign access as part of your deployment.
