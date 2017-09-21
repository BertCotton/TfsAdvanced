using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
using TFSAdvanced.DataStore.Interfaces;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;
using TFSAdvanced.Updater.Models.PullRequests;
using TFSAdvanced.Updater.Models.Repositories;
using TFSAdvanced.Updater.Tasks;
using PullRequest = TFSAdvanced.Models.DTO.PullRequest;
using Repository = TFSAdvanced.Models.DTO.Repository;
using Reviewer = TFSAdvanced.Models.DTO.Reviewer;

namespace TfsAdvanced.Updater.Tasks
{
    public class PullRequestUpdater : UpdaterBase
    {
        protected readonly RequestData requestData;
        private readonly PullRequestRepository pullRequestRepository;
        private readonly RepositoryRepository repositoryRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly BuildRepository buildRepository;
        private DateTime lastCompletedRequest;
        
        public PullRequestUpdater(PullRequestRepository pullRequestRepository, RequestData requestData, RepositoryRepository repositoryRepository, 
            UpdateStatusRepository updateStatusRepository, BuildRepository buildRepository,
            ILogger<PullRequestUpdater> logger) : base(logger)
        {
            this.requestData = requestData;
            this.repositoryRepository = repositoryRepository;
            this.updateStatusRepository = updateStatusRepository;
            this.buildRepository = buildRepository;
            this.pullRequestRepository = pullRequestRepository;
        }

        protected override async Task Update(bool initialize)
        {
            if (initialize && !pullRequestRepository.IsEmpty())
                return;

            int pullRequestCount = 0;
            foreach (var repository in repositoryRepository.GetAll())
            {

                if (string.IsNullOrEmpty(repository.PullRequestUrl))
                    return;
                IList<PullRequest> pullRequests = new List<PullRequest>();
                foreach (var pullRequest in GetPullRequests(repository))
                {
                    CommitLink commitId = null;
                    if (pullRequest.lastMergeCommit != null)
                        commitId = pullRequest.lastMergeCommit;
                    else if (pullRequest.lastMergeSourceCommit != null)
                        commitId = pullRequest.lastMergeSourceCommit;

                    if (commitId == null)
                    {
                        logger.LogWarning($"Unable to get last merge commit for the pullrequest ({pullRequest.pullRequestId}) {pullRequest.description}");
                        return;
                    }

                    if (string.IsNullOrEmpty(commitId.commitId))
                    {
                        logger.LogWarning($"Unable to get the last commitID for the pull request ({pullRequest.pullRequestId}) {pullRequest.description}");
                        return;
                    }
                    var build = buildRepository.GetBuildBySourceVersion(repository, commitId.commitId);


                    var pullRequestDto = BuildPullRequest(pullRequest, build);
                    pullRequestDto.Repository = repository;
                    pullRequestDto.Url = BuildPullRequestUrl(pullRequest, requestData.BaseAddress);
                    pullRequestDto.RequiredReviewers = repository.MinimumApproverCount;

                    pullRequests.Add(pullRequestDto);
                }
                await pullRequestRepository.Update(pullRequests);
                pullRequestCount = pullRequests.Count;
            }

            
            updateStatusRepository.UpdateStatus(new UpdateStatus {LastUpdate = DateTime.Now, UpdatedRecords = pullRequestCount, UpdaterName = GetType().Name});
            lastCompletedRequest = DateTime.Now;

        }

        protected virtual IList<TFSAdvanced.Updater.Models.PullRequests.PullRequest> GetPullRequests(Repository repository)
        {
            List<TFSAdvanced.Updater.Models.PullRequests.PullRequest> pullRequests = new List<TFSAdvanced.Updater.Models.PullRequests.PullRequest>();

            var activePullRequests = GetAsync.FetchResponseList<TFSAdvanced.Updater.Models.PullRequests.PullRequest>(requestData, repository.PullRequestUrl).Result;
            if(activePullRequests != null)
                pullRequests.AddRange(activePullRequests);

            var completedPullRequests = GetAsync.FetchResponseList<TFSAdvanced.Updater.Models.PullRequests.PullRequest>(requestData, repository.PullRequestUrl + "?status=Completed").Result;
            if (completedPullRequests != null)
            {
                var past2Days = DateTime.Now.Date.AddDays(-2);
                if (lastCompletedRequest > DateTime.MinValue)
                    past2Days = lastCompletedRequest.AddMinutes(-5);
                // Only show the completed pull requests that have been completed in the past two days
                pullRequests.AddRange(completedPullRequests.Where(x => x.creationDate >= past2Days).ToList());
            }
            return pullRequests;
        }


        private PullRequest BuildPullRequest(TFSAdvanced.Updater.Models.PullRequests.PullRequest x, Build build)
        {
            PullRequest pullRequestDto = new PullRequest
            {
                Id = x.pullRequestId,
                Title = x.title,
                Url = x.remoteUrl,
                CreatedDate = x.creationDate,
                ClosedDate = x.closedDate,
                Creator = new User
                {
                    UniqueName = x.createdBy.uniqueName,
                    IconUrl = x .createdBy.imageUrl,
                    Name = x.createdBy.displayName
                },
                Repository =repositoryRepository.GetById(x.repository.id),
                MergeStatus = x.mergeStatus == "conflicts" ? MergeStatus.Failed : MergeStatus.Succeeded,
                IsAutoCompleteSet = x.completionOptions != null,
                HasEnoughReviewers = x.hasEnoughReviewers,
                AcceptedReviewers = x.acceptedReviewers,
                RequiredReviewers = x.requiredReviewers,
                LastCommit = x.lastMergeSourceCommit.commitId,
                Reviewers = new List<Reviewer>()
            };

            if (x.reviewers != null)
            {
                foreach (var reviewer in x.reviewers)
                {
                    if (reviewer.isContainer)
                        continue;

                    // Only ignore the review of the creator if the vote is approved or noresponse
                    if (reviewer.uniqueName == x.createdBy.uniqueName && (reviewer.vote == (int)Vote.Approved || reviewer.vote == (int)Vote.NoResponse))
                        continue;


                    ReviewStatus reviewStatus = ReviewStatus.NoResponse;
                    switch ((Vote)reviewer.vote)
                    {
                        case Vote.Approved:
                            reviewStatus = ReviewStatus.Approved;
                            if (reviewer.uniqueName != x.createdBy.uniqueName)
                                pullRequestDto.AcceptedReviewers++;
                            break;
                        case Vote.ApprovedWithSuggestions:
                            reviewStatus = ReviewStatus.ApprovedWithSuggestions;
                            break;
                        case Vote.NoResponse:
                            reviewStatus = ReviewStatus.NoResponse;
                            break;
                        case Vote.Rejected:
                            reviewStatus = ReviewStatus.Rejected;
                            break;
                        case Vote.WaitingForAuthor:
                            reviewStatus = ReviewStatus.WaitingForAuthor;
                            break;
                    }
                    
                    var reviewerDto = new Reviewer
                    {
                        User = new User
                        {
                            UniqueName = reviewer.uniqueName,
                            IconUrl = reviewer.imageUrl,
                            Name = reviewer.displayName
                        },
                        PullRequest = pullRequestDto
                    };

                    reviewerDto.ReviewStatus = reviewStatus;

                    pullRequestDto.Reviewers.Add(reviewerDto);
                }
            }

            if (build == null)
            {
                pullRequestDto.BuildStatus = BuildStatus.NoBuild;
            }
            else
            {
                pullRequestDto.buildId = build.Id;
                pullRequestDto.BuildUrl = build.Url;
                pullRequestDto.BuildStatus = build.BuildStatus;
            }

            return pullRequestDto;
        }


        public string BuildPullRequestUrl(TFSAdvanced.Updater.Models.PullRequests.PullRequest pullRequest, string baseUrl)
        {
            return
                $"{baseUrl}/{pullRequest.repository.project.name}/_git/{pullRequest.repository.name}/pullrequest/{pullRequest.pullRequestId}?view=files";
        }
    }
}
