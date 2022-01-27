namespace AutoScrum.Core.Config;

/// <summary>
/// Separate configurations for storing app config so that we don't need to write all of the projects,
/// every time user changes their project.
/// </summary>
public class AppConfig
{
    public string? SelectedProject { get; set; }
    public ThemeSettings? ThemeSettings { get; set; }
}
