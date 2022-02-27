using AutoScrum.Core.Config;
using AutoScrum.Infrastructure.Blazor.Persistence.Containers;
using Blazored.LocalStorage;

namespace AutoScrum.Infrastructure.Blazor.Persistence.Migrations;

internal class ProjectsMetadataMigration : IMigration<ProjectsMetadataContainer, List<ProjectMetadata>>
{
    private readonly ILocalStorageService _localStorage;

    public ProjectsMetadataMigration(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<ProjectsMetadataContainer?> Migrate(ProjectsMetadataContainer? container)
    {
        if (container?.Version == 1 && container.Item?.Count > 1)
        {
            List<int> checkedIds = new();
            List<ProjectMetadata> itemsToRemove = new();
            
            foreach (var item in container.Item)
            {
                if (!checkedIds.Contains(item.Id))
                {
                    checkedIds.Add(item.Id);
                }
                else
                {
                    itemsToRemove.Add(item);
                }
            }

            foreach (var itemToRemove in itemsToRemove)
            {
                container.Item.Remove(itemToRemove);
            }

            container.Version = 2;
        }

        if (container != null)
        {
            await _localStorage.SetItemAsync(container.StorageTableName, container);
        }

        return container;
    }
}
