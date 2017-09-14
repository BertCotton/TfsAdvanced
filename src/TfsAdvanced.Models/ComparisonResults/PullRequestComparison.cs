using System.Collections.Generic;

namespace TFSAdvanced.Models.ComparisonResults
{
    public class PullRequestComparison
    {
        public bool IsNewPullRequest { get; set; }
        
        public bool ReviewersUpdated { get; set; }
        
        public bool BuildStatusUpdated { get; set; }
        
        public bool MergeStatusUpdated { get; set; }
        
        public bool FilesChanged { get; set; }
        
        public IList<string> Messages { get; set; } = new List<string>();
    }
}