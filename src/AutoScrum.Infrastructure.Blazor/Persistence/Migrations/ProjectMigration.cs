using AutoScrum.Infrastructure.Blazor.Models;
using AutoScrum.Infrastructure.Blazor.Persistence.Containers;

namespace AutoScrum.Infrastructure.Blazor.Persistence.Migrations;

internal class ProjectMigration : IMigration<ProjectContainer, ProjectStorageItem>
{
    public Task<ProjectContainer?> Migrate(ProjectContainer? container) => Task.FromResult(container);
}
