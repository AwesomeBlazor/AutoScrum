using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AutoScrum
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

			builder.Services
				.AddBlazorise(options =>
				{
					options.ChangeTextOnKeyPress = true;
				})				
				.AddBootstrapProviders()
				.AddFontAwesomeIcons();

			var host = builder.Build();

			//host.Services
			//  .UseBootstrapProviders()
			//  .UseFontAwesomeIcons();

			await host.RunAsync();
        }
    }
}
