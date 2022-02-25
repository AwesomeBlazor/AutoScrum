using AutoScrum.Core.Config;
using AutoScrum.Infrastructure.Blazor.Persistence.Containers;
using AutoScrum.Infrastructure.Blazor.Persistence.Migrations;
using Blazored.LocalStorage;

namespace AutoScrum.Infrastructure.Blazor.Persistence;

internal class ProjectsMetadataRepo
{
    private readonly ILocalStorageService _localStorage;

    public ProjectsMetadataRepo(ILocalStorageService localStorageService)
    {
        _localStorage = localStorageService;
    }

    public async Task<List<ProjectMetadata>> GetProjects()
    {
        ProjectsMetadataContainer? container = await _localStorage.GetItemAsync<ProjectsMetadataContainer>(ProjectsMetadataContainer.StorageKey);
        container = await container.Migrate(() => new ProjectsMetadataMigration());

        return container?.Item
            ?? new List<ProjectMetadata>();
    }

    public async Task AddOrUpdate(ProjectMetadata projectMetadata)
    {
        var container = await _localStorage.GetItemAsync<ProjectsMetadataContainer>(ProjectsMetadataContainer.StorageKey);

        container ??= new();
        container.Item ??= new();
        container.Item.Add(projectMetadata);
        await _localStorage.SetItemAsync(ProjectsMetadataContainer.StorageKey, container);
    }

    public async Task<string?> Remove(ProjectMetadata projectMetadata)
    {
        var container = await _localStorage.GetItemAsync<ProjectsMetadataContainer>(ProjectsMetadataContainer.StorageKey);
        if (container?.Item?.Any() != true)
        {
            return null;
        }

        var itemToRemove = container.Item.Find(x => projectMetadata.Id == x.Id);
        if (itemToRemove == null)
        {
            return null;
        }

        container.Item.Remove(itemToRemove);
        return itemToRemove.Path;
    }
}
