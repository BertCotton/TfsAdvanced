namespace TfsAdvanced.Models.Repositories
{
    public class Commit
    {
        public Person author { get; set; }
        public ChangeCounts changecounts { get; set; }
        public string comment { get; set; }
        public string commitId { get; set; }
        public Person committer { get; set; }
    }
}