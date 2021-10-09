using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AutoScrum.Infrastructure.Blazor.Persistence.Containers
{
    internal abstract class StorageContainer<T>
    {
        public int Version { get; set; }

        [JsonIgnore]
        public virtual bool ShouldMigrate { get; }

        [JsonIgnore]
        public string StorageTableName { get; internal set; } = null!;

        public T? Item { get; set; }
    }
}
