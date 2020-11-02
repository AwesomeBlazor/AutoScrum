namespace AutoScrum.AzureDevOps.Models
{
    public class WorkItem
    {
        public int? Id { get; set; }
        public string? IterationPath { get; set; }
        public string? Type { get; set; }
        public string? State { get; set; }
        public string? Title { get; set; }
        public string? StateChangeDateString { get; set; }
        public string Url { get; set; }
    }
}
