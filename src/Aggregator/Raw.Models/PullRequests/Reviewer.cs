namespace TFSAdvanced.Aggregator.Raw.Models.PullRequests
{
    public class Reviewer
    {
        public string displayName { get; set; }
        public string id { get; set; }
        public string imageUrl { get; set; }
        public bool isContainer { get; set; }
        public string reviewerUrl { get; set; }
        public string uniqueName { get; set; }
        public string url { get; set; }
        public int vote { get; set; }
        public VotedFor[] votedFor { get; set; }
    }
}