using AutoScrum.Core.Services;
using AutoScrum.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AutoScrum.AzureDevOps;

public static class DependencyInjection
{
    public static IServiceCollection AddAzureDevOps(this IServiceCollection services)
        => services
            .AddSingleton<IDailyScrumGenerator, AzureDevOpsDailyScrumGenerator>();
}
