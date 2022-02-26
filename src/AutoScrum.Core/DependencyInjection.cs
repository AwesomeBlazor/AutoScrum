using AutoScrum.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AutoScrum.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
        => services
            .AddTransient<IDailyScrumService, DailyScrumService>()
            .AddTransient<IDateService, DateService>();
}
