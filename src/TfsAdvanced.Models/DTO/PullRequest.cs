using System;
using System.Collections.Generic;

namespace TFSAdvanced.Models.DTO
{
    public class PullRequest
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public Repository Repository { get; set; }

        public BuildStatus BuildStatus { get; set; }

        public string BuildUrl { get; set; }

        public MergeStatus MergeStatus { get; set; }

        public int buildId { get; set; }
        
        public bool IsAutoCompleteSet { get; set; }

        public bool HasEnoughReviewers { get; set; }

        public User Creator { get; set; }

        public IList<Reviewer> Reviewers { get; set; }

        public int RequiredReviewers { get; set; }

        public int AcceptedReviewers { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
