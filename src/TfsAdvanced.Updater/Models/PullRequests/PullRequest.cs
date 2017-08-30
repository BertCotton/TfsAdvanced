using System;
using TfsAdvanced.Models;
using TFSAdvanced.Updater.Models.Repositories;

namespace TFSAdvanced.Updater.Models.PullRequests
{
    public class PullRequest
    {
        public PullRequestLinks _links { get; set; }
        public Createdby createdBy { get; set; }
        public DateTime creationDate { get; set; }
        public DateTime? closedDate { get; set; }
        public string description { get; set; }
        public CommitLink lastMergeCommit { get; set; }
        public CommitLink lastMergeSourceCommit { get; set; }
        public CommitLink lastMergeTargetCommit { get; set; }
        public string mergeId { get; set; }
        public string status { get; set; }
        public string mergeStatus { get; set; }
        public int pullRequestId { get; set; }
        public string remoteUrl { get; set; }
        public Repository repository { get; set; }
        public Reviewer[] reviewers { get; set; }
        public string sourceRefName { get; set; }
        public string targetRefName { get; set; }
        public string title { get; set; }
        public string url { get; set; }

        public int requiredReviewers { get; set; }

        public int acceptedReviewers { get; set; }

        public bool hasEnoughReviewers => acceptedReviewers >= requiredReviewers;

        public CompletionOptions completionOptions { get; set; }
        

    }
}