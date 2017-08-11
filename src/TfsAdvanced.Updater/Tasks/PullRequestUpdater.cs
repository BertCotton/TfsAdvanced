using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
using TFSAdvanced.Models.DTO;
using TFSAdvanced.Updater.Models.PullRequests;
using TFSAdvanced.Updater.Tasks;
using PullRequest = TFSAdvanced.Models.DTO.PullRequest;
using Reviewer = TFSAdvanced.Models.DTO.Reviewer;

namespace TfsAdvanced.Updater.Tasks
{
    public class PullRequestUpdater : UpdaterBase
    {
        private readonly RequestData requestData;
        private readonly PullRequestRepository pullRequestRepository;
        private readonly RepositoryRepository repositoryRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly BuildRepository buildRepository;
        
        public PullRequestUpdater(PullRequestRepository pullRequestRepository, RequestData requestData, RepositoryRepository repositoryRepository, 
            UpdateStatusRepository updateStatusRepository, BuildRepository buildRepository, ILogger<PullRequestUpdater> logger) : base(logger)
        {
            this.requestData = requestData;
            this.repositoryRepository = repositoryRepository;
            this.updateStatusRepository = updateStatusRepository;
            this.buildRepository = buildRepository;
            this.pullRequestRepository = pullRequestRepository;
        }

        protected override void Update()
        {
            ConcurrentBag<PullRequest> allPullRequests = new ConcurrentBag<PullRequest>();
            Parallel.ForEach(repositoryRepository.GetAll(), new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, repository =>
            {
                if (string.IsNullOrEmpty(repository.PullRequestUrl))
                    return;
                var pullRequests = GetAsync.FetchResponseList<TFSAdvanced.Updater.Models.PullRequests.PullRequest>(requestData, repository.PullRequestUrl).Result;
                if (pullRequests == null)
                    return;
                Parallel.ForEach(pullRequests, new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, pullRequest =>
                {
                    try
                    {
                        if (pullRequest.lastMergeCommit == null)
                        {
                            logger.LogWarning($"Unable to get last merge commit for the pullrequest ({pullRequest.pullRequestId}) {pullRequest.description}");
                            return;
                        }

                        if (string.IsNullOrEmpty(pullRequest.lastMergeCommit.commitId))
                        {
                            logger.LogWarning($"Unable to get the last commitID for the pull request ({pullRequest.pullRequestId}) {pullRequest.description}");
                            return;
                        }
                        var build = buildRepository.GetBuildBySourceVersion(repository, pullRequest.lastMergeCommit.commitId);


                        var pullRequestDto = BuildPullRequest(pullRequest, build);
                        pullRequestDto.Repository = repository;
                        pullRequestDto.Url = BuildPullRequestUrl(pullRequest, requestData.BaseAddress);
                        pullRequestDto.RequiredReviewers = repository.MinimumApproverCount;

                        foreach (var reviewer in pullRequest.reviewers)
                        {
                            // Container reviewers do not count
                            if (reviewer.isContainer)
                                continue;
                            if (reviewer.vote == (int) Vote.Approved)
                                pullRequestDto.AcceptedReviewers++;
                        }
                        allPullRequests.Add(pullRequestDto);
                    }
                    catch (Exception e)
                    {
                        logger.LogError("Error parsing pull request", e);
                    }
                });
            });
            var pullRequestsList = allPullRequests.ToList();
            pullRequestRepository.Update(pullRequestsList);
            updateStatusRepository.UpdateStatus(new UpdateStatus {LastUpdate = DateTime.Now, UpdatedRecords = pullRequestsList.Count, UpdaterName = nameof(PullRequestUpdater)});

        }

        private PullRequest BuildPullRequest(TFSAdvanced.Updater.Models.PullRequests.PullRequest x, Build build)
        {
            PullRequest pullRequestDto = new PullRequest
            {
                Id = x.pullRequestId,
                Title = x.title,
                Url = x.remoteUrl,
                CreatedDate = x.creationDate,
                Creator = new User
                {
                    Name = x.createdBy.displayName,
                    IconUrl = x.createdBy.imageUrl
                },
                Repository = new Repository
                {
                    Name = x.repository.name,
                    Url = x.repository.remoteUrl,
                    Project = new Project
                    {
                        Name = x.repository.project.name,
                        Url = x.repository.project.remoteUrl
                    }
                },
                MergeStatus = x.mergeStatus == "conflicts" ? MergeStatus.Failed : MergeStatus.Succeeded,
                IsAutoCompleteSet = x.completionOptions != null,
                HasEnoughReviewers = x.hasEnoughReviewers,
                AcceptedReviewers = x.acceptedReviewers,
                RequiredReviewers = x.requiredReviewers,
                Reviewers = new List<Reviewer>()
            };

            if (x.reviewers != null)
            {
                foreach (var reviewer in x.reviewers)
                {
                    if (reviewer.isContainer)
                        continue;
                    var reviewerDto = new Reviewer
                    {
                        Name = reviewer.displayName,
                        IconUrl = reviewer.imageUrl

                    };
                    switch ((Vote)reviewer.vote)
                    {
                        case Vote.Approved:
                            reviewerDto.ReviewStatus = ReviewStatus.Approved;
                            break;
                        case Vote.ApprovedWithSuggestions:
                            reviewerDto.ReviewStatus = ReviewStatus.ApprovedWithSuggestions;
                            break;
                        case Vote.NoResponse:
                            reviewerDto.ReviewStatus = ReviewStatus.NoResponse;
                            break;
                        case Vote.Rejected:
                            reviewerDto.ReviewStatus = ReviewStatus.Rejected;
                            break;
                        case Vote.WaitingForAuthor:
                            reviewerDto.ReviewStatus = ReviewStatus.WaitingForAuthor;
                            break;
                    }
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
