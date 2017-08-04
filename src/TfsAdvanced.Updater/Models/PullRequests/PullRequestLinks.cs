using TfsAdvanced.Models;

namespace TFSAdvanced.Updater.Models.PullRequests
{
    public class PullRequestLinks
    {
        public HrefLink createdBy { get; set; }
        public HrefLink repository { get; set; }
        public HrefLink self { get; set; }
        public HrefLink sourceBranch { get; set; }
        public HrefLink sourceCommit { get; set; }
        public HrefLink targetBranch { get; set; }
        public HrefLink targetCommit { get; set; }
        public HrefLink workItems { get; set; }
    }
}