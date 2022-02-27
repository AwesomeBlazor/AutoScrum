namespace AutoScrum.Core.Config;

public abstract class ProjectConfig
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string ProjectName { get; set; } = null!;
    public ProjectType ProjectType { get; set; }
    public int Version { get; set; } = 1;
}