using AutoScrum.Core.Config;
using AutoScrum.Core.Services;
using AutoScrum.Infrastructure.Blazor.Persistence;
using Microsoft.Extensions.Caching.Memory;

namespace AutoScrum.Infrastructure.Blazor.Services;

internal class ConfigService : IConfigService
{
    public const string AppConfigCacheKey = "AppConfig";
    public const string ProjectsMetaCacheKey = "ProjectsMeta";

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
        return await _memoryCache.GetOrCreateAsync(AppConfigCacheKey, async entity =>
        {
            var config = await _appConfigRepo.GetAppConfig();
            entity.SetAbsoluteExpiration(TimeSpan.FromMinutes(config != null ? 5 : 0.1));

            return config;
        }) ?? new AppConfig();
    }

    public async Task<ThemeSettings> GetTheme()
    {
        AppConfig? config = await GetAppConfig();

        return config?.ThemeSettings ?? new();
    }

    public async Task SetTheme(ThemeSettings theme)
    {
        AppConfig? config = await GetAppConfig();
        config.ThemeSettings = theme;

        await UpdateConfig(config);
    }

    public async Task SetCurrentProject(int projectId)
    {
        AppConfig? config = await GetAppConfig();
        if (config.SelectedProjectId == projectId)
        {
            return;
        }

        config.SelectedProjectId = projectId;
        await UpdateConfig(config);
    }

    public async Task<ProjectConfig?> GetCurrentProject()
    {
        AppConfig? config = await GetAppConfig();
        ProjectMetadata? projectMeta = await GetProjectMetadata(config.SelectedProjectId);
        return await _projectRepo.Get(projectMeta?.Path);
    }

    public async Task<List<ProjectMetadata>> GetProjectsMetadata()
    {
        return await _memoryCache.GetOrCreateAsync(ProjectsMetaCacheKey, async entity =>
        {
            var projects = await _projectsMetadataRepo.GetProjects();
            entity.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            return projects;
        }) ?? new List<ProjectMetadata>();
    }

    public async Task<TProjectConfig> AddOrUpdateProject<TProjectConfig>(TProjectConfig project)
        where TProjectConfig : ProjectConfig
    {
        List<ProjectMetadata> projectsMeta = await GetProjectsMetadata();
        ProjectMetadata? projectMetadata = projectsMeta.Find(x => x.Id == project.Id);
        if (project.Id == 0 || projectMetadata == null)
        {
            // Max fails if there are no values.
            project.Id = projectsMeta.Count > 0
                ? projectsMeta.Max(x => x.Id) + 1
                : 1;
            project.ProjectName ??= "default";
            projectMetadata = new()
            {
                Id = project.Id,
                Name = project.ProjectName,
                Path = project.Id.ToString(),
                ProjectType = project.ProjectType
            };
        }

        return await AddOrUpdateProject(projectMetadata, project);
    }

    public async Task<TProjectConfig> AddOrUpdateProject<TProjectConfig>(ProjectMetadata projectMetadata, TProjectConfig project)
        where TProjectConfig : ProjectConfig
    {
        await _projectsMetadataRepo.AddOrUpdate(projectMetadata);
        await _projectRepo.AddOrUpdate(projectMetadata, project);

        _memoryCache.Remove(ProjectsMetaCacheKey);

        return project;
    }

    public async Task RemoveProject(ProjectMetadata projectMetadata)
    {
        string? path = await _projectsMetadataRepo.Remove(projectMetadata);
        await _projectRepo.Remove(path);

        _memoryCache.Remove(ProjectsMetaCacheKey);
    }

    public async Task RemoveProject(ProjectConfig project)
    {
        var projectsMeta = await GetProjectsMetadata();
        var projectToRemove = projectsMeta.Find(x => x.Id == project.Id);
        if (projectToRemove != null)
        {
            await RemoveProject(projectToRemove);
        }
    }

    private async Task UpdateConfig(AppConfig config)
    {
        await _appConfigRepo.SaveAppConfig(config);

        _memoryCache.Remove(AppConfigCacheKey);
    }

    private async Task<ProjectMetadata?> GetProjectMetadata(int projectId)
    {
        List<ProjectMetadata>? projectsMeta = await GetProjectsMetadata();
        return projectsMeta.Find(x => x.Id == projectId);
    }
}
