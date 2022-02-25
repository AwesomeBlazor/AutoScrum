using AutoScrum.Core.Config;
using AutoScrum.Infrastructure.Blazor.Models;
using AutoScrum.Infrastructure.Blazor.Persistence.Containers;
using AutoScrum.Infrastructure.Blazor.Persistence.Migrations;
using Blazored.LocalStorage;
using System.Text.Json;

namespace AutoScrum.Infrastructure.Blazor.Persistence;

internal class ProjectRepo
{
    private readonly ILocalStorageService _localStorage;

    public ProjectRepo(ILocalStorageService localStorageService)
    {
        _localStorage = localStorageService;
    }

    public Task<ProjectConfig?> GetProject(ProjectMetadata projectMetadata) => Get(projectMetadata?.Path);

    public async Task<ProjectConfig?> Get(string? path)
    {
        Console.WriteLine($"Loading project - {path}");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        ProjectContainer? container = await _localStorage.GetItemAsync<ProjectContainer>(ProjectContainer.StorageKey(path));
        container = await container.Migrate(() => new ProjectMigration());

        ProjectConfig? project = null;
        if (container?.Item != null)
        {
            Console.WriteLine("Parsing project - " + container.Item.ProjectType + " - " + container.Item.JsonPayload);
            ProjectStorageItem? storageItem = container.Item;
            if (storageItem.ProjectType == ProjectType.AzureDevOps)
            {
                project = JsonSerializer.Deserialize<ProjectConfigAzureDevOps>(storageItem.JsonPayload);
            }
        }

        return project;
    }

    public async Task AddOrUpdate<TProjectConfig>(ProjectMetadata metadata, TProjectConfig projectConfig)
        where TProjectConfig : ProjectConfig
    {
        if (metadata?.Path == null)
        {
            return;
        }

        // Atm, update simply overrides current data.
        string path = ProjectContainer.StorageKey(metadata.Path);
        ProjectStorageItem storageItem = new()
        {
            ProjectType = projectConfig.ProjectType,
            JsonPayload = JsonSerializer.Serialize(projectConfig),
        };
        await _localStorage.SetItemAsync(path, new ProjectContainer(path, storageItem));
    }

    public async Task Remove(string? path)
    {
        if (path == null)
        {
            return;
        }

        string projectPath = ProjectContainer.StorageKey(path);
        if (await _localStorage.ContainKeyAsync(projectPath))
        {
            await _localStorage.RemoveItemAsync(projectPath);
        }
    }
}
