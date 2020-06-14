appsettings.local.json
======================

The `appsettings.local.json` file is excluded from source control, so in order to run this sample you need to create that file in the same folder with this documentation.

The contents of that file should contain the following settings that specify how your application will authenticate with the configured authority, which can be any authority that supports Open ID Connect, such as [Azure AD](https://azure.microsoft.com/en-gb/services/active-directory/) or [Azure AD B2C](https://azure.microsoft.com/en-gb/services/active-directory/customer-partner-identity/).

``` JSON
{
  "oidc": {
    "Authority": string,
    "ClientId": string,
    "CallbackPath": "/signin-oidc"
  }
}
```

Properties
----------

The configuration needs the following properties that are specified in the JSON document above.

|Name|Description|
|----|-----------|
|Authority|The authorization endpoint to send the users to for authentication. See the Authority chapter below for details how this is constructed for Azure AD and Azure AD B2C.|
|ClientId|The ID of the application. In Azure AD, this is a `Guid`, but for instance with many other authorities, the ClientId is something else.|
|CallbackPath|This is the path that the authority will return the result of the authentication, which is either the requested token or an error code of some sort. You can always specify it as `/signin-oidc` for both Azure AD and Azure AD B2C.|



### Authority

Depending on what authority you want to use to authenticate your users, the Authority is constructed in different ways. The authority is always a URL that users are sent to for authentication. The chapters below show how to construct the authorization URL for Azure AD and Azure AD B2C. If you want to use another authority, you should be able to get the authorization endpoint from the documentation for that system.


#### Azure AD

The `Authority` setting for Azure AD looks like this:
`https://login.microsoftonline.com/[tenant name].onmicrosoft.com`

If you want to use v2.0 of the enpoint, `Authority` URL looks like this:
`https://login.microsoftonline.com/[tenant name].onmicrosoft.com/v2.0`

So, if the name of your tenant is `contoso`, then the v2 URL would be:
`https://login.microsoftonline.com/contoso.onmicrosoft.com/v2.0`

You can also replace `[tenant name].onmicrosoft.com` with the ID of the tenant. If your application is a multi-tenant application that needs to allow any Azure AD tenant to authenticate, you use `common` as the name for your tenant.


See [this Microsoft Authentication Library article](https://docs.microsoft.com/bs-latn-ba/azure/active-directory/develop/msal-client-application-configuration) for details on how to construct the `Authority` URL.

The metadata endpoint for Azure AD tenants is found at:

`https://login.microsoftonline.com/[tenant name].onmicrosoft.com/.well-known/openid-configuration`

or v2 metadata endpoint at:

https://login.microsoftonline.com/[tenant name].onmicrosoft.com/v2.0/.well-known/openid-configuration


#### Azure AD B2C

With Azure AD B2C, the `Authority` URL is a bit different. It looks like this:

`https://[tenant name].b2clogin.com/tfp/[tenant name].onmicrosoft.com/[policy ID]/v2.0`

In Azure AD B2C you can define different user flows (policies), and you need to specify the ID of the policy in the `Authority` URL. The policy you specify is the policy you want your users to use when signing in to your application.

> Note! If you want to enable your users to also sign up for your application and create themselves an account they can use to sign in with, the sign-in policy needs to also support sign-up. This is also true if you are just using Azure AD B2C as a federation partner, and using social logins or Open ID Connect logins with your application. These users will also need to be able to sign up even though they are not creating regular accounts in your Azure AD B2C tenant.

The metadata enpoint for Azure AD B2C tenants is found at:

`https://[tenant name].b2clogin.com/tfp/[tenant name].onmicrosoft.com/[policy ID]/v2.0/.well-known/openid-configuration`
