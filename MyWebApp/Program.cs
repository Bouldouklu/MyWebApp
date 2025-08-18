using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyWebApp;
using MyWebApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient with proper base address
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register services with proper lifetimes
builder.Services.AddSingleton<CoffeeService>();
builder.Services.AddScoped<WeatherService>();
builder.Services.AddScoped<NewsService>();
builder.Services.AddScoped<IGNReviewsService>();

await builder.Build().RunAsync();