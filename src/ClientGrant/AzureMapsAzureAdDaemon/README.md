# Sample .NET Core Console Application with Azure Maps and Azure AD

 The technical difference is this sample shows REST API call using [Azure Service Authentication Library C#](https://docs.microsoft.com/en-us/azure/key-vault/service-to-service-authentication). This library can be used with [Azure Managed Identities](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview) or a crafted Azure AD application registration. The Azure AD authentication flow implemented in this sample is referred to as [client credentials grant](https://docs.microsoft.com/en-us/azure/active-directory/develop/v1-oauth2-client-creds-grant-flow).
 Client credentials grant is suitable for some scenarios where no interactive user sign in should be enabled through Azure AD. This could be because the application has a desire to implment a seperate form of authentication from Azure AD. Please read to confirm that it meets your requirements.

## Steps to configure

1. Create a new .Net Core Console Application from Visual Studio template.
2. Create new application registration in [Azure Portal](https://portal.azure.com/) for Azure AD directory.
3. Create an Azure Maps Account resource.
4. Assign users, groups, or applications to Azure Maps Access Management.

### Details

#### Creating new console application

Why: we need to host application.

1. Simply create a new Console application from Visual Studio and modify the `Program.cs`.
2. Using Azure Service Authentication Library the code can use an application registration in the next step.
3. Craft any HTTP request with the `Authorization` header and the `x-ms-client-id` header. With those 2 headers you can authenticate and authorize with Azure Maps REST API.

#### Creating new Application Registration in Azure AD (for Local Development)

Why: we must model and represent the application in Azure AD.

1. Azure AD has documentation on [how to register an application](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app). Go with option "Accounts in this organizational directory only". In this specific sample, we do not require a redirect uri.
2. The application should be granted access to Azure Maps. In 'API Permissions', add 'Azure Maps' with the 'user_impersonation' scope under [delegated permission](https://docs.microsoft.com/en-us/azure/active-directory/develop/v1-permissions-and-consent).
3. Next step is to create a secret to authenticate to Azure AD, this can be secret string.
4. Finally, capture the [application (client) id and the tenant](https://docs.microsoft.com/en-us/azure/active-directory/develop/app-registrations-training-guide#new-ui). The application id should be a `guid` and it represents the app in Azure AD. The tenant should be in the form of `mytenant.onmicrosoft.com`. With all these values you can create a connection string `RunAs=App;AppId=[application id];TenantId=[mytenant.onmicrosoft.com];AppKey=[secret string]`
5. Create an Windows / System environment variable named `AzureServicesAuthConnectionString` with the connection string created above. Read more on [Azure Service Authentication Library C#](https://docs.microsoft.com/en-us/azure/key-vault/service-to-service-authentication).

#### Create an Azure Maps Account Resource

1. Refer to the parent `README.md` for the created resource.
2. Once the resource has been created, go to the authentication blade and copy the [client id property](https://docs.microsoft.com/en-us/azure/azure-maps/how-to-manage-authentication#view-authentication-details). This `guid` should be used in the `Maps.cshtml` page to indicate the Map account to be used by your application.

#### Assigning Users or Groups to the Azure Maps Resource

Why: we need to grant access to the Azure Map account. The application created by the application registration and Managed Identity should be given access.

1. On the Azure Portal with the previously created Azure Maps resource. Navigate to the `access control (IAM)` blade. On this UI, you can add a role assignment. What this does is grant the user which you search for access to a pre-defined role of permssions for Azure Maps.
2. Search for the application registration by display name.
3. Select Azure Maps Data Reader role definition.
4. Save the role assignment.
   - The order of creating this role assignment was defined because we assigned on the Azure Maps Resource. However a higher level of access can be created at the Resource Group or Subsubscription scopes. Please read [Role Based Access Control](https://docs.microsoft.com/en-us/azure/role-based-access-control/role-definitions#data-operations-example) to invesigate your needs.
   - Tip: you can use, [Azure Resource Manager Template](https://docs.microsoft.com/en-us/azure/role-based-access-control/role-assignments-template) to also assign access as part of your deployment.
