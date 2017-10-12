using System.Linq;
using TFSAdvanced.Models.ComparisonResults;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.Data.Comparator
{
    public class PullRequestComparator
    {
        public static PullRequestComparison Compare(PullRequest original, PullRequest updated)
        {
            PullRequestComparison comparison = new PullRequestComparison();
            if (original == null)
            {
                comparison.IsNewPullRequest = true;
                return comparison;
            }

            if (original.buildId != updated.buildId)
            {
                comparison.BuildStatusUpdated = true;
                comparison.Messages.Add($"Build has changed from {original.buildId} to {updated.buildId}.");
            }

            if (original.BuildStatus != updated.BuildStatus)
            {
                comparison.BuildStatusUpdated = true;
                comparison.Messages.Add($"Build Status has changed from {original.BuildStatus} to {updated.BuildStatus}.");
            }

            if (original.LastCommit != updated.LastCommit)
            {
                comparison.FilesChanged = true;
                comparison.Messages.Add($"Files have been updated from commit {original.LastCommit} to {updated.LastCommit}");
            }


            if (original.MergeStatus != updated.MergeStatus)
            {
                comparison.MergeStatusUpdated = true;
                comparison.Messages.Add($"Merge status changed from {original.MergeStatus} to {original.MergeStatus}");
            }


            foreach (var reviewer in original.Reviewers)
            {
                var updatedReviewer = updated.Reviewers.FirstOrDefault(x => x.UniqueName == reviewer.UniqueName);
                if (updatedReviewer == null)
                {
                    comparison.ReviewersUpdated = true;
                    comparison.Messages.Add($"{reviewer.Name} removed as a reviewer");
                }
                else if (reviewer.ReviewStatus != updatedReviewer.ReviewStatus)
                {
                    comparison.ReviewersUpdated = true;
                    comparison.Messages.Add($"{reviewer.Name} changed review status from {reviewer.ReviewStatus} to {updatedReviewer.ReviewStatus}");
                }

            }
            foreach (var updatedReviewer in updated.Reviewers)
            {
                if (original.Reviewers.Any(x => x.UniqueName == updatedReviewer.UniqueName))
                    continue;

                comparison.ReviewersUpdated = true;
                comparison.Messages.Add($"{updatedReviewer.Name} added as reviewer with status {updatedReviewer.ReviewStatus}");
            }

            return comparison;
        }
    }
}