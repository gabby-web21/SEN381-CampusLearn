using CampusLearnFrontend;
using CampusLearnFrontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Root components
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:5001/")
});

// Register your custom services
builder.Services.AddSingleton<NavStateService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<FakeDataService>();

builder.Services.AddAuthorizationCore();

// CustomAuthStateProvider is the actual provider
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());

// AuthService depends on CustomAuthStateProvider
builder.Services.AddScoped<AuthService>();

#if DEBUG
// Enable detailed logging in Debug mode
builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

await builder.Build().RunAsync();
