namespace AutoScrum.Models;

public class ThemeSettings
{
    public AntTheme Theme { get; set; }
}

public enum AntTheme
{
    Default,
    Dark,
    Aliyun,
    Compact
}