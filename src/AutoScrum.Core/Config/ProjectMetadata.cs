namespace AutoScrum.Core.Config;

public class ProjectMetadata
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ProjectType ProjectType { get; set; }
    public string Path { get; set; }

    public void Copy(ProjectMetadata projectMetadata)
    {
        Id = projectMetadata.Id;
        Name = projectMetadata.Name;
        ProjectType = projectMetadata.ProjectType;
        Path = projectMetadata.Path;
    }
}