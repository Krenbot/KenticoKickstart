using System;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Membership;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using Kentico.Xperience.Cloud;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NovusOne;
using NovusOne.Web.Features.Navigation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddXperienceCloudApplicationInsights(builder.Configuration);

if (
    builder.Environment.IsQa()
    || builder.Environment.IsUat()
    || builder.Environment.IsEnvironment(CloudEnvironments.Custom)
    || builder.Environment.IsEnvironment(CloudEnvironments.Staging)
    || builder.Environment.IsProduction()
)
{
    builder.Services.AddKenticoCloud(builder.Configuration);
    builder.Services.AddXperienceCloudSendGrid(builder.Configuration);
    builder.Services.AddXperienceCloudDataProtection(builder.Configuration);
}

builder.Services.AddKentico(features =>
{
    features.UsePageBuilder(
        new PageBuilderOptions { ContentTypeNames = [LandingPage.CONTENT_TYPE_NAME] }
    );
    features.UseWebPageRouting();
});

builder.Services.AddAuthentication();

// ADD SSO CONFIGURATION
builder.Services.AddAdminExternalAuthenticationProvider(
    authBuilder =>
        authBuilder.AddMicrosoftIdentityWebApp(options =>
        {
            options.Domain = "";
            options.TenantId = builder.Configuration["EntraTenantId"];
            options.ClientId = builder.Configuration["EntraClientId"];
            options.ClientSecret = builder.Configuration["EntraClientSecret"];
            options.Instance = "https://login.microsoftonline.com/";
            options.CallbackPath = new PathString("/admin-oidc");
            options.ResponseType = OpenIdConnectResponseType.Code;
        }),
    options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.AuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;

        // Map to the actual claim names received from Entra ID
        options.UserNameClaimName = "preferred_username";
        options.EmailClaimName = "email";
        options.FirstNameClaimName = "given_name";
        options.LastNameClaimName = "family_name";

        // Map the correct Microsoft role claim type
        options.RoleClaimName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        // Xperience synchronizes the user account with data from the external provider every time a user signs in
        options.UserSynchronizationFrequency = UserSynchronizationFrequency.Always;
    }
);

builder.Services.AddControllersWithViews();

// KEEP YOUR NAVIGATION SERVICE
builder.Services.AddSingleton<INavigationService, NavigationService>();

var app = builder.Build();
app.InitKentico();

app.UseStaticFiles();

app.UseCookiePolicy();

app.UseAuthentication();

if (
    builder.Environment.IsQa()
    || builder.Environment.IsUat()
    || builder.Environment.IsEnvironment(CloudEnvironments.Custom)
    || builder.Environment.IsEnvironment(CloudEnvironments.Staging)
    || builder.Environment.IsProduction()
)
{
    app.UseKenticoCloud();
}

app.UseKentico();

// KEEP YOUR WORKING ROUTING
app.Kentico().MapRoutes();

app.Run();
