using AutoScrum.Core.Config;
using AutoScrum.Infrastructure.Blazor.Persistence.Containers;

namespace AutoScrum.Infrastructure.Blazor.Persistence.Migrations;

internal class ProjectsMetadataMigration : IMigration<ProjectsMetadataContainer, List<ProjectMetadata>>
{
    public Task<ProjectsMetadataContainer?> Migrate(ProjectsMetadataContainer? container) => Task.FromResult(container);
}
