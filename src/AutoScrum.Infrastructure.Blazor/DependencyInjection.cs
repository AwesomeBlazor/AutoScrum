using AutoScrum.Core.Services;
using AutoScrum.Infrastructure.Blazor.Persistence;
using AutoScrum.Infrastructure.Blazor.Services;
using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;

namespace AutoScrum.Infrastructure.Blazor
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBlazorInfrastructure(this IServiceCollection services)
        {
            // Services and repos.
            services
                .AddTransient<IConfigService, ConfigService>()
                .AddTransient<AppConfigRepo>()
                .AddTransient<ProjectRepo>()
                .AddTransient<ProjectsMetadataRepo>();

            // 3rd party dependencies.
            services
                .AddBlazoredLocalStorage()
                .AddMemoryCache();

            return services;
        }
    }
}
