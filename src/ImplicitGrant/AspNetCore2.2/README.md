# Sample Aspnet Core Web App with Azure Maps and Azure AD

 This sample shows a DHTML (Razor Views) with a client-driven user login experience. This Azure AD authentication flow is referred to as [implicit grant](https://docs.microsoft.com/en-us/azure/active-directory/develop/v1-oauth2-implicit-grant-flow). Implicit grant is suitable for when no server component is available for authentication. Please read the documentation to confirm that it meets your requirements. If you try running this sample Azure AD will challenge saying you do not belong to the configured Azure AD tenant / directory. You **must** configure your own tenant and applications and update the configuration.

## Steps to configure

1. Create a new Web App Asp.Net Core from Visual Studio template.
2. Create new application registration in [Azure Portal](https://portal.azure.com/) for Azure AD directory.
3. Create an Azure Maps Account resource.
4. Assign users, groups, or applications to Azure Maps Access Management.

### Details

#### Creating new web application

1. When using Azure AD, it is **critical** to secure your application with an SSL certificate. Visual Studio tools provide AspNetCore applications with a developer SSL certificate which you must trust when prompted during development / debugging of the application.
1. For simplicity, we've created a new page called `Maps.cshtml` and added it to the `Layout.cshtml` to host the Web SDK.

#### Creating new Application Registration in Azure AD

Why: we must model and represent the application in Azure AD.
  
1. In this specific sample, we must add a redirect uri to the Application Registration for `https://localhost:5001/Home/Maps` because we will invoke the authentication sign in from this route.
2. Now the application should be granted access to Azure Maps. In 'API Permissions', add 'Azure Maps' with the 'user_impersonation' scope under delegated permission. To better understand permission and consent read [here](https://docs.microsoft.com/en-us/azure/active-directory/develop/v1-permissions-and-consent).
3. Next step is to configure the registration to enable implicit grant. To enable the implicit grant you must modify the application registration in the [Authentication page](https://docs.microsoft.com/en-us/azure/active-directory/develop/app-registrations-training-guide#new-ui). This authentication not turned on by default because in most cases Microsoft recommend using [Open ID Connect](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-sign-user-app-registration?tabs=aspnetcore).
4. Finally, capture the [application (client) id and the tenant](https://docs.microsoft.com/en-us/azure/active-directory/develop/app-registrations-training-guide#new-ui). The application id should be a `guid` and it represent the application in Azure AD. The tenant should be in the form of `mytenant.onmicrosoft.com`. Replace the values in `Maps.cshtml` with these.

#### Create an Azure Maps Account Resource

Why: we need Azure Maps account resource to authorize with.

1. Refer to the parent `README.md` for the Azure Maps resource.
2. Once the resource has been created, go to the authentication blade and copy the [client id property](https://docs.microsoft.com/en-us/azure/azure-maps/how-to-manage-authentication#view-authentication-details). This `guid` should be used in the `Maps.cshtml` page to indicate the Map account to be used by your application.

#### Assigning Users or Groups to the Azure Maps Resource

Why: we need to grant the right people or groups of people access to the Azure Maps account. These people should already exist in the Azure AD tenant and be added to the group which you specified from Azure AD. Read more about Azure AD groups [here](https://docs.microsoft.com/en-us/azure/active-directory/fundamentals/active-directory-manage-groups).

1. On the Azure Portal with the created Azure Maps resource. Navigate to the `access control (IAM)` blade. On this UI, you can add a role assignment. This will grant the user access to a pre-defined role of permssions for Azure Maps.
2. Search for the user or group by name or email and select them.
3. Select Azure Maps Data Reader role definition.
4. Save the role assignment.
   - The order of creating this role assignment was defined because we assigned on the Azure Maps Resource. However a higher scope of access can be created at the Resource Group or Subsubscription. Please read [Role Based Access Control](https://docs.microsoft.com/en-us/azure/role-based-access-control/role-definitions#data-operations-example) to futher meet your needs.
   - Tip: you can use, [Azure Resource Manager Template](https://docs.microsoft.com/en-us/azure/role-based-access-control/role-assignments-template) to also assign access as part of your deployment.
