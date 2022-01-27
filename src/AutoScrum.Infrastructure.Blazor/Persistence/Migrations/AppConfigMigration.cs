using AutoScrum.Core.Config;
using AutoScrum.Infrastructure.Blazor.Models;
using AutoScrum.Infrastructure.Blazor.Persistence.Containers;
using Blazored.LocalStorage;
using System.Text.Json;

namespace AutoScrum.Infrastructure.Blazor.Persistence.Migrations;

internal class AppConfigMigration : IMigration<AppConfigContainer, AppConfig>
{
    private readonly ILocalStorageService _localStorage;

    public AppConfigMigration(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<AppConfigContainer?> Migrate(AppConfigContainer? container)
    {
        string originalConfigKey = "current-config";
        string originalThemeKey = "current-theme";

        container ??= new AppConfigContainer();
        container.Item ??= new AppConfig();

        var config = await _localStorage.GetItemAsync<ProjectConfigAzureDevOps>(originalConfigKey);
        if (config != null)
        {
            container.Item.SelectedProject = "default";

            ProjectsMetadataContainer metadataContainer = new()
            {
                Item = new List<ProjectMetadata>
                    {
                        new ProjectMetadata
                        {
                            Name = "Default",
                            Path = "default",
                            ProjectType = ProjectType.AzureDevOps
                        }
                    }
            };

            await _localStorage.SetItemAsync(metadataContainer.StorageTableName, metadataContainer);

            ProjectContainer projectContainer = new()
            {
                Item = new ProjectStorageItem
                {
                    ProjectType = ProjectType.AzureDevOps,
                    JsonPayload = JsonSerializer.Serialize(config)
                },
                Path = "default",
                StorageTableName = ProjectContainer.StorageKey("default")
            };

            await _localStorage.SetItemAsync(projectContainer.StorageTableName, projectContainer);
        }

        var theme = await _localStorage.GetItemAsync<ThemeSettings>(originalThemeKey);
        if (theme != null)
        {
            container.Item.ThemeSettings = theme;
        }

        container.Version = ProjectsMetadataContainer.CurrentVersion;

        // TODO: Save this when fully tested.

        return container;
    }
}
