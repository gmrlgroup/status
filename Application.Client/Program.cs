using Application.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Application.Shared.Services;
using Application.Client.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddFluentUIComponents();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();


builder.Services.AddScoped(http => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<ClientAuthenticationDetail>();
builder.Services.AddScoped<StateContainer>();

// Register client services
builder.Services.AddScoped<EntityClientService>();
builder.Services.AddScoped<IncidentClientService>();
builder.Services.AddScoped<EntityStatusHistoryClientService>();

//builder.Services.AddScoped<IExerciseService, ExerciseService>();
//builder.Services.AddScoped<IPlanExerciseService, PlanExerciseService>();

await builder.Build().RunAsync();
