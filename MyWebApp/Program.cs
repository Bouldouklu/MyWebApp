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
builder.Services.AddScoped<CoffeeService>();
builder.Services.AddScoped<WeatherService>();
builder.Services.AddScoped<NewsService>();
builder.Services.AddScoped<GameDevNewsService>();
builder.Services.AddScoped<RugbyNewsService>();

// 🆕 Register the new dynamic rugby calendar service
builder.Services.AddScoped<RugbyCalendarService>();

// 🆕 Register the Todo service
builder.Services.AddSingleton<TodoService>();

await builder.Build().RunAsync();