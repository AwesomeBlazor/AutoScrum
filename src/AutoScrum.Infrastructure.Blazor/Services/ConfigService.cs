using AutoScrum.Core.Config;
using AutoScrum.Core.Services;
using AutoScrum.Infrastructure.Blazor.Persistence;
using Microsoft.Extensions.Caching.Memory;

namespace AutoScrum.Infrastructure.Blazor.Services
{
    internal class ConfigService : IConfigService
    {
        public const string AppConfigCacheKey = "AppConfig";

        private readonly AppConfigRepo _appConfigRepo;
        private readonly ProjectsMetadataRepo _projectsMetadataRepo;
        private readonly ProjectRepo _projectRepo;
        private readonly IMemoryCache _memoryCache;

        public ConfigService(AppConfigRepo appConfigRepo, ProjectsMetadataRepo projectsMetadataRepo, ProjectRepo projectRepo, IMemoryCache memoryCache)
        {
            _appConfigRepo = appConfigRepo;
            _projectsMetadataRepo = projectsMetadataRepo;
            _projectRepo = projectRepo;
            _memoryCache = memoryCache;
        }

        public async Task<AppConfig> GetAppConfig()
        {
            if (_memoryCache.TryGetValue(AppConfigCacheKey, out AppConfig? config) && config != null)
            {
                return config;
            }

            config = await _appConfigRepo.GetAppConfig();
            if (config != null)
            {
                _memoryCache.Set(AppConfigCacheKey, config, TimeSpan.FromMinutes(5));
            }

            return config
                ?? new AppConfig();
        }

        public async Task<ThemeSettings> GetTheme()
        {
            AppConfig? config = await GetAppConfig();

            return config?.ThemeSettings ?? new();
        }

        public async Task SetTheme(ThemeSettings theme) { }

        public async Task<ProjectConfig?> GetCurrentProject()
        {
            ProjectConfig? project = null;
            AppConfig? config = await GetAppConfig();
            if (config != null)
            {
                string selectedProject = config.SelectedProject ?? "default";
                project = await _projectRepo.GetProject(selectedProject);
            }

            return project;
        }
    }
}
