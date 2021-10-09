using AutoScrum.Core.Config;
using System.Threading.Tasks;

namespace AutoScrum.Core.Services
{
    public interface IConfigService
    {
        Task<AppConfig> GetAppConfig();
        Task<ThemeSettings> GetTheme();
        Task SetTheme(ThemeSettings theme);

        Task<ProjectConfig?> GetCurrentProject();
    }
}
