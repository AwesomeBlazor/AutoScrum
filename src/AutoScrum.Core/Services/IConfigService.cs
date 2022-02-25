using AutoScrum.Core.Config;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoScrum.Core.Services;

public interface IConfigService
{
    Task<AppConfig> GetAppConfig();
    Task<ThemeSettings> GetTheme();
    Task SetTheme(ThemeSettings theme);

    Task<List<ProjectMetadata>> GetProjectsMetadata();
    Task<ProjectConfig?> GetCurrentProject();
    Task SetCurrentProject(int projectId);
    Task<TProjectConfig> AddOrUpdateProject<TProjectConfig>(ProjectMetadata projectMetadata, TProjectConfig project) where TProjectConfig : ProjectConfig;
    Task<TProjectConfig> AddOrUpdateProject<TProjectConfig>(TProjectConfig project) where TProjectConfig : ProjectConfig;
    Task RemoveProject(ProjectMetadata projectMetadata);
    Task RemoveProject(ProjectConfig project);
}