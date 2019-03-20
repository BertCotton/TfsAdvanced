namespace TFSAdvanced.Aggregator.Raw.Models.JobRequests
{
    public class Agent
    {
        public int id { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public bool enabled { get; set; }
        public string status { get; set; }
    }
}
