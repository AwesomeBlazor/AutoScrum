using AutoScrum.Core.Config;
using AutoScrum.Infrastructure.Blazor.Persistence.Containers;
using AutoScrum.Infrastructure.Blazor.Persistence.Migrations;
using Blazored.LocalStorage;

namespace AutoScrum.Infrastructure.Blazor.Persistence;

internal class AppConfigRepo
{
    private readonly ILocalStorageService _localStorage;

    public AppConfigRepo(ILocalStorageService localStorageService)
    {
        _localStorage = localStorageService;
    }

    public async Task<AppConfig?> GetAppConfig()
    {
        AppConfigContainer? container = await _localStorage.GetItemAsync<AppConfigContainer>(AppConfigContainer.StorageKey);
        container = await container.Migrate(() => new AppConfigMigration(_localStorage));

        return container?.Item;
    }

    public async Task SaveAppConfig(AppConfig appConfig)
    {
        await _localStorage.SetItemAsync(AppConfigContainer.StorageKey, new AppConfigContainer(appConfig));
    }
}
