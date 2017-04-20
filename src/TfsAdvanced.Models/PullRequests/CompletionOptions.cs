using System;
using System.Collections.Generic;
using System.Text;

namespace Models.PullRequests
{
    public class CompletionOptions
    {
        public string mergeCommitMessage { get; set; }

        public bool deleteSourceBranch { get; set; }
    }
}
