namespace AutoScrum.Core.Config;

public class ProjectMetadata
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ProjectType ProjectType { get; set; }
    public string Path { get; set; }
}