using Application.Components;
using Application.Components.Account;
using Application.Helpers;
using Application.Shared.Data;
using Application.Shared.Models;
using Application.Shared.Models.User;
using Application.Shared.Services;
using Application.Shared.Services.Org;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;



var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("DuckDB.NET.Native.DisableLibraryLoad", false);


builder.Services.AddControllers();


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddAuthenticationStateSerialization()
    .AddInteractiveWebAssemblyComponents()
    .AddInteractiveServerComponents();




builder.Services.AddFluentUIComponents();

builder.Services.AddCascadingAuthenticationState();
//builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();

builder.Services.AddApiAuthorization();

const string MS_OIDC_SCHEME = "MicrosoftOidc";

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;


    }).AddCookie("Identity.Application")
    .AddCookie("Identity.External")
    .AddOpenIdConnect(MS_OIDC_SCHEME, displayName: "Continue with Microsoft", options =>
     {

         options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

         options.SignInScheme = IdentityConstants.ExternalScheme;
         options.ClientId = builder.Configuration["AzureAd:ClientId"];
         options.ClientSecret = builder.Configuration["AzureAd:ClientSecret"];
         options.Authority = builder.Configuration["AzureAd:Authority"];
         options.MetadataAddress = builder.Configuration["AzureAd:MetadataAddress"];
         options.CallbackPath = builder.Configuration["AzureAd:CallbackPath"];
         options.RequireHttpsMetadata = false;

         options.SaveTokens = true;
         options.GetClaimsFromUserInfoEndpoint = true;

         options.SignedOutRedirectUri = builder.Configuration["AzureAd:SignedOutRedirectUri"];
         options.SignedOutCallbackPath = builder.Configuration["AzureAd:SignedOutCallbackPath"];
         options.ResponseType = OpenIdConnectResponseType.Code;


         // .NET 9 feature
         options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
         options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
         options.TokenValidationParameters.RoleClaimType = "role";

     });

var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContext") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContext' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Application")));



builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddRoles<IdentityRole>()
    .AddRoleManager<RoleManager<IdentityRole>>()
    .AddRoleStore<RoleStore<IdentityRole, ApplicationDbContext>>()
    .AddUserStore<UserStore<ApplicationUser, IdentityRole, ApplicationDbContext>>()
    .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddMemoryCache();


builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddScoped<StateContainer>();
builder.Services.AddScoped<ClientAuthenticationDetail>();

builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEntityService, EntityService>();
builder.Services.AddScoped<IIncidentService, IncidentService>();


// Add EmailSettings configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Add AuthenticationSettings configuration
builder.Services.Configure<AuthenticationSettings>(builder.Configuration.GetSection("Authentication"));

// Add HTTP client factory
builder.Services.AddHttpClient();

// Register EmailHelper as a singleton
builder.Services.AddSingleton<EmailHelper>();


builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
    options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
    //options.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;
    //options.User.RequireUniqueEmail = true;

});

// get the uri from the appsettings.json
var uri = builder.Configuration["BaseUri"];
//// Configure the HttpClient to include the user's access token when calling the API
builder.Services.AddHttpClient("Application.ServerAPI", client => client.BaseAddress = new Uri(uri));

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Application.ServerAPI"));


builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();

app.MapControllers();

app.UseStaticFiles();
app.UseAntiforgery();


// app.MapHub<NotificationHub<DataJob>>("/notification/datajob");
app.UseResponseCompression();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Application.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
