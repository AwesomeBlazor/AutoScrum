using AutoScrum.Core.Config;
using AutoScrum.Infrastructure.Blazor.Persistence.Containers;
using AutoScrum.Infrastructure.Blazor.Persistence.Migrations;
using Blazored.LocalStorage;
using System.Text.Json;

namespace AutoScrum.Infrastructure.Blazor.Persistence
{
    internal class ProjectRepo
    {
        private readonly ILocalStorageService _localStorage;

        public ProjectRepo(ILocalStorageService localStorageService)
        {
            _localStorage = localStorageService;
        }

        public Task<ProjectConfig?> GetProject(ProjectMetadata projectMetadata) => GetProject(projectMetadata?.Path);

        public async Task<ProjectConfig?> GetProject(string? path)
        {
            Console.WriteLine("Loading project - " + path);
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
                var storageItem = container.Item;
                if (storageItem.ProjectType == ProjectType.AzureDevOps)
                {
                    project = JsonSerializer.Deserialize<ProjectConfigAzureDevOps>(storageItem.JsonPayload);
                }
            }

            return project;
        }
    }
}
