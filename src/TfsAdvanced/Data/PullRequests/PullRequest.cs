using System;
using TfsAdvanced.Data.Repositories;

namespace TfsAdvanced.Data.PullRequests
{
    public class PullRequest
    {
        public PullRequestLinks _links { get; set; }
        public Createdby createdBy { get; set; }
        public DateTime creationDate { get; set; }
        public string description { get; set; }
        public CommitLink lastMergeCommit { get; set; }
        public CommitLink lastMergeSourceCommit { get; set; }
        public CommitLink lastMergeTargetCommit { get; set; }
        public string mergeId { get; set; }
        public string mergeStatus { get; set; }
        public string mergeStatusstatus { get; set; }
        public int pullRequestId { get; set; }
        public string remoteUrl { get; set; }
        public Repositories.Repository repository { get; set; }
        public Reviewer[] reviewers { get; set; }
        public string sourceRefName { get; set; }
        public string targetRefName { get; set; }
        public string title { get; set; }
        public string url { get; set; }
    }
}