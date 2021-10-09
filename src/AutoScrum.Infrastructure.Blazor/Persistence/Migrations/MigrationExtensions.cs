using AutoScrum.Infrastructure.Blazor.Persistence.Containers;

namespace AutoScrum.Infrastructure.Blazor.Persistence.Migrations
{
    internal static class MigrationExtensions
    {
        public static async Task<TStorageContainer?> Migrate<TStorageContainer, TContainerType>(this TStorageContainer? container, Func<IMigration<TStorageContainer, TContainerType>> migrationFactory)
            where TStorageContainer : StorageContainer<TContainerType>
        {
            if (container == null || container.ShouldMigrate)
            {
                IMigration<TStorageContainer, TContainerType>? migration = migrationFactory();
                if (migration != null)
                {
                    container = await migration.Migrate(container);
                }
            }

            return container;
        }
    }
}
