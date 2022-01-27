using AutoScrum.Core.Config;
using AutoScrum.Infrastructure.Blazor.Models;
using System.Text.Json.Serialization;

namespace AutoScrum.Infrastructure.Blazor.Persistence.Containers;

internal class ProjectContainer : StorageContainer<ProjectStorageItem>
{
    public const int CurrentVersion = 1;

    public ProjectContainer()
    {
        Version = CurrentVersion;
    }

    public ProjectContainer(string path)
    {
        Version = CurrentVersion;
        StorageTableName = StorageKey(path);
    }

    public string? Path { get; set; }

    public static string StorageKey(string path) => "Project-" + path;

    [JsonIgnore]
    public override bool ShouldMigrate => Version < CurrentVersion;
}
