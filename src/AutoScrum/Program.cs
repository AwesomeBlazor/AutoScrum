using AutoScrum.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;

namespace AutoScrum
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
	        var builder = WebAssemblyHostBuilder.CreateDefault(args);
	        builder.RootComponents.Add<App>("#app");
	        builder.RootComponents.Add<HeadOutlet>("head::after");

	        var services = builder.Services;

	        services.AddScoped(_ => new HttpClient
	        {
		        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
	        });
	        services.AddTransient<ConfigService>();
	        services.AddTransient<AutoMessageService>();

	        services.AddBlazoredLocalStorage();
	        services.AddAntDesign();

	        var host = builder.Build();

	        await host.RunAsync();

        }
    }
}
