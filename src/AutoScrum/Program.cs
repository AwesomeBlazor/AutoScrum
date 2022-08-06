using AutoScrum.AzureDevOps;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services
	.AddTransient<OldConfigService>()
	.AddTransient<AutoMessageService>()
	.AddTransient<IClipboardService, ClipboardService>()
	.AddBlazorInfrastructure()
	.AddAzureDevOps()
	.AddCore();

builder.Services.AddAntDesign();

await builder.Build().RunAsync();
