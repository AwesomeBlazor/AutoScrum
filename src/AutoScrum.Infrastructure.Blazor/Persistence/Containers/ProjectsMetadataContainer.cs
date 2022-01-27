using AutoScrum.Core.Config;
using System.Text.Json.Serialization;

namespace AutoScrum.Infrastructure.Blazor.Persistence.Containers;

internal class ProjectsMetadataContainer : StorageContainer<List<ProjectMetadata>>
{
    public const string StorageKey = "ProjectsMetadata";
    public const int CurrentVersion = 1;

    public ProjectsMetadataContainer()
    {
        Version = CurrentVersion;
        StorageTableName = StorageKey;
    }

    [JsonIgnore]
    public override bool ShouldMigrate => Version < CurrentVersion;
}
