using AutoScrum.Infrastructure.Blazor.Persistence.Containers;

namespace AutoScrum.Infrastructure.Blazor.Persistence.Migrations
{
    internal interface IMigration<TStorageContainer, TContainerType>
        where TStorageContainer : StorageContainer<TContainerType>
    {
        Task<TStorageContainer?> Migrate(TStorageContainer? container);
    }
}
