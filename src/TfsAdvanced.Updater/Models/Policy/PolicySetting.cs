namespace TFSAdvanced.Updater.Models.Policy
{
    public class PolicySetting
    {
        public int buildDefinitionId { get; set; }
        public int minimumApproverCount { get; set; }
        public bool creatorVoteCountes { get; set; }
        public bool queueOnSourceUpdateOnly { get; set; }
        public bool manualQueueOnly { get; set; }
        public string displayName { get; set; }
        public long validDuration { get; set; }
        public string[] requiredReviewerIds { get; set; }
        public string[] filenamePattersn { get; set; }
        public bool addedFilesOnly { get; set; }
        public string message { get; set; }
        public PolicyScope[] scope { get; set; }
    }
}
