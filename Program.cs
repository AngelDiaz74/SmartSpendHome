using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SmartSpendHome;
using SmartSpendHome.Services;
using SmartSpendHome.Models;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddBlazoredLocalStorage();

builder.Services.Configure<SupabaseSettings>(
    builder.Configuration.GetSection("Supabase"));

builder.Services.AddScoped<ISupabaseAuthService, SupabaseAuthService>();
builder.Services.AddScoped<ISupabaseApiService, SupabaseApiService>();


builder.Services.AddScoped<IShoppingListService, SupabaseShoppingListService>();
builder.Services.AddScoped<IGroceryBudgetService, SupabaseGroceryBudgetService>();
builder.Services.AddScoped<IReceiptParserService, ReceiptParserService>();

await builder.Build().RunAsync();
