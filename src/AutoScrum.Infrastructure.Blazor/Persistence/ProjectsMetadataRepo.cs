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

    public async Task<List<ProjectMetadata>> GetAppConfig()
    {
        ProjectsMetadataContainer? container = await _localStorage.GetItemAsync<ProjectsMetadataContainer>(ProjectsMetadataContainer.StorageKey);
        container = await container.Migrate(() => new ProjectsMetadataMigration());

        return container?.Item
            ?? new List<ProjectMetadata>();
    }
}
