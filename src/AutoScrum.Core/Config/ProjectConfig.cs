namespace AutoScrum.Core.Config
{
    public abstract class ProjectConfig
    {
        public string ProjectName { get; set; }
        public ProjectType ProjectType { get; set; }
        public int Version { get; set; } = 1;
    }
}
