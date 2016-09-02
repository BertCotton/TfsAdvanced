namespace TfsAdvanced.Data.Projects
{
    public class Project
    {
        public ProjectLinks _links { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string remoteUrl { get; set; }
        public string state { get; set; }
        public string url { get; set; }
    }
}