using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TfsAdvanced.Data.Comparator;
using TFSAdvanced.DataStore;
using TFSAdvanced.DataStore.Interfaces;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.ComparisonResults;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class PullRequestRepository : SqlRepositoryBase<PullRequest>
    {
        public PullRequestRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public IList<PullRequest> GetPullRequestsAfter(int id, bool onlyActive = true)
        {
            if(onlyActive)
                return base.GetList(x => x.Id > id && !x.ClosedDate.HasValue);
            else
                return base.GetList(x => x.Id > id);
        }

        public override DateTime GetLastUpdated()
        {
            using (var context = new TfsAdvancedSqlDataContext(configuration))
            {
                return context.PullRequests.Where(x => !x.ClosedDate.HasValue).Select(x => x.LastUpdated).OrderByDescending(x => x).FirstOrDefault();
            }
        }

        public override IList<PullRequest> GetAll()
        {
            using (var context = new TfsAdvancedSqlDataContext(configuration))
            {
                return GetSet(context).Where(x => !x.ClosedDate.HasValue).ToList();
            }
        }

        protected override bool Matches(PullRequest source, PullRequest target)
        {
            return source.Id == target.Id;
        }

        protected override IQueryable<PullRequest> AddIncludes(DbSet<PullRequest> data)
        {
            return data.Include(request => request.Repository).ThenInclude(repository => repository.Project)
                .Include(request => request.Creator)
                .Include(request => request.Reviewers).ThenInclude(reviewer => reviewer.User);
        }

        public override void AttachProperties(TfsAdvancedSqlDataContext dataContext, PullRequest pullRequest)
        {
            pullRequest.Repository = dataContext.Repositories.FirstOrDefault(x => x.Id == pullRequest.Repository.Id);
            pullRequest.Creator = GetOrUpdate(dataContext, x => x.UniqueName == pullRequest.Creator.UniqueName, pullRequest.Creator);

            if (pullRequest.Reviewers?.Any() == true)
            {
                IList<Reviewer> reviewers = new List<Reviewer>(pullRequest.Reviewers.Count);
                foreach (var pullRequestReviewer in pullRequest.Reviewers)
                {
                    if (pullRequestReviewer.Id > 0)
                    {
                        reviewers.Add(dataContext.Reviewers.FirstOrDefault(x => x.Id == pullRequestReviewer.Id));
                    }
                    else
                    {
                        pullRequestReviewer.User = GetOrUpdate(dataContext, x => x.UniqueName == pullRequestReviewer.User.UniqueName, pullRequestReviewer.User);
                        reviewers.Add(pullRequestReviewer);
                    }
                }
                pullRequest.Reviewers = reviewers;
            }
        }

        protected override void Map(PullRequest from, PullRequest to)
        {
            to.AcceptedReviewers = from.AcceptedReviewers;
            to.BuildStatus = from.BuildStatus;
            to.buildId = from.buildId;
            to.BuildUrl = from.BuildUrl;
            to.ClosedDate = from.ClosedDate;
            to.HasEnoughReviewers = from.HasEnoughReviewers;
            to.IsAutoCompleteSet = from.IsAutoCompleteSet;
            to.LastCommit = from.LastCommit;
            to.MergeStatus = from.MergeStatus;
            to.Reviewers = from.Reviewers;
            to.RequiredReviewers = from.RequiredReviewers;
            to.Title = from.Title;
            to.Reviewers = from.Reviewers;
        }
    }
    
}
