namespace TfsAdvanced.Models.Projects
{
    public class ProjectLinks
    {
        public HrefLink commits { get; set; }
        public HrefLink items { get; set; }
        public HrefLink project { get; set; }
        public HrefLink pullRequests { get; set; }
        public HrefLink pushes { get; set; }
        public HrefLink refs { get; set; }
        public HrefLink self { get; set; }
        public HrefLink web { get; set; }
    }
}