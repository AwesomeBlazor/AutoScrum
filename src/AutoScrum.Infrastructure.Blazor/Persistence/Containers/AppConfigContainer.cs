using AutoScrum.Core.Config;
using System.Text.Json.Serialization;

namespace AutoScrum.Infrastructure.Blazor.Persistence.Containers;

internal class AppConfigContainer : StorageContainer<AppConfig>
{
    public const string StorageKey = "AppConfig";
    public const int CurrentVersion = 1;

    public AppConfigContainer()
    {
        Version = CurrentVersion;
        StorageTableName = StorageKey;
    }

    public AppConfigContainer(AppConfig appConfig)
        : this()
    {
        Item = appConfig;
    }

    [JsonIgnore]
    public override bool ShouldMigrate => Version < CurrentVersion;
}
