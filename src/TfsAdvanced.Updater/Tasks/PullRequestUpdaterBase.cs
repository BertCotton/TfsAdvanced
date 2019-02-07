using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
using TFSAdvanced.DataStore.Interfaces;
using TFSAdvanced.Models.DTO;
using TFSAdvanced.Updater.Models.PullRequests;
using TFSAdvanced.Updater.Models.Repositories;
using TFSAdvanced.Updater.Tasks;
using PullRequest = TFSAdvanced.Models.DTO.PullRequest;
using Repository = TFSAdvanced.Models.DTO.Repository;
using Reviewer = TFSAdvanced.Models.DTO.Reviewer;

namespace TfsAdvanced.Updater.Tasks
{
    public abstract class PullRequestUpdaterBase : UpdaterBase
    {
        private readonly BuildRepository buildRepository;
        private readonly RepositoryRepository repositoryRepository;
        private readonly UpdateStatusRepository updateStatusRepository;

        protected PullRequestUpdaterBase(IPullRequestRepository pullRequestRepository, RequestData requestData, RepositoryRepository repositoryRepository,
            UpdateStatusRepository updateStatusRepository, BuildRepository buildRepository, ILogger<PullRequestUpdaterBase> logger) : base(logger)
        {
            RequestData = requestData;
            this.repositoryRepository = repositoryRepository;
            this.updateStatusRepository = updateStatusRepository;
            this.buildRepository = buildRepository;
            PullRequestRepository = pullRequestRepository;
        }

        protected IPullRequestRepository PullRequestRepository { get; }

        protected RequestData RequestData { get; }

        public string BuildPullRequestUrl(TFSAdvanced.Updater.Models.PullRequests.PullRequest pullRequest, string baseUrl)
        {
            return
                $"{baseUrl}/{pullRequest.repository.project.name}/_git/{pullRequest.repository.name}/pullrequest/{pullRequest.pullRequestId}?view=files";
        }

        protected virtual IList<TFSAdvanced.Updater.Models.PullRequests.PullRequest> GetPullRequests(Repository repository)
        {
            return GetAsync.FetchResponseList<TFSAdvanced.Updater.Models.PullRequests.PullRequest>(RequestData, repository.PullRequestUrl, Logger).Result;
        }

        protected override void Update()
        {
            var pullRequestCount = 0;

            Parallel.ForEach(repositoryRepository.GetAll(), new ParallelOptions { MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM }, repository =>
              {
                  var repositoryPullRequests = new ConcurrentBag<PullRequest>();
                  if (string.IsNullOrEmpty(repository.PullRequestUrl))
                      return;
                  IList<TFSAdvanced.Updater.Models.PullRequests.PullRequest> pullRequests = GetPullRequests(repository);
                  if (pullRequests == null)
                      return;
                  Parallel.ForEach(pullRequests, new ParallelOptions { MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM }, pullRequest =>
                  {
                      try
                      {
                          CommitLink commitId = null;
                          if (pullRequest.lastMergeCommit != null)
                              commitId = pullRequest.lastMergeCommit;
                          else if (pullRequest.lastMergeSourceCommit != null)
                              commitId = pullRequest.lastMergeSourceCommit;

                          if (commitId == null)
                          {
                              Logger.LogWarning($"Unable to get last merge commit for the pullrequest ({pullRequest.pullRequestId}) {pullRequest.description}");
                              return;
                          }

                          if (string.IsNullOrEmpty(commitId.commitId))
                          {
                              Logger.LogWarning($"Unable to get the last commitID for the pull request ({pullRequest.pullRequestId}) {pullRequest.description}");
                              return;
                          }
                          Build build = buildRepository.GetBuildBySourceVersion(repository, commitId.commitId);

                          PullRequest pullRequestDto = BuildPullRequest(pullRequest, build);
                          pullRequestDto.Repository = repository;
                          pullRequestDto.Url = BuildPullRequestUrl(pullRequest, RequestData.BaseAddress);
                          pullRequestDto.RequiredReviewers = repository.MinimumApproverCount;

                          foreach (TFSAdvanced.Updater.Models.PullRequests.Reviewer reviewer in pullRequest.reviewers)
                          {
                              // Container reviewers do not count
                              if (reviewer.isContainer)
                                  continue;
                              if (reviewer.vote == (int)Vote.Approved)
                                  pullRequestDto.AcceptedReviewers++;
                          }
                          pullRequestDto.LastUpdated = DateTime.Now;
                          repositoryPullRequests.Add(pullRequestDto);
                          Interlocked.Increment(ref pullRequestCount);
                      }
                      catch (Exception e)
                      {
                          Logger.LogError(e, "Error parsing pull request");
                      }
                  });
                  if (repositoryPullRequests.Any())
                      PullRequestRepository.Update(repositoryPullRequests.ToList());
              });
            updateStatusRepository.UpdateStatus(new UpdateStatus { LastUpdate = DateTime.Now, UpdatedRecords = pullRequestCount, UpdaterName = GetType().Name });
        }

        private PullRequest BuildPullRequest(TFSAdvanced.Updater.Models.PullRequests.PullRequest x, Build build)
        {
            var pullRequestDto = new PullRequest
            {
                Id = x.pullRequestId,
                Title = x.title,
                Url = x.remoteUrl,
                ApiUrl = x.url,
                CreatedDate = x.creationDate,
                ClosedDate = x.closedDate,
                Creator = new User
                {
                    Name = x.createdBy.displayName,
                    IconUrl = x.createdBy.imageUrl,
                    UniqueName = x.createdBy.uniqueName
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
                LastCommit = x.lastMergeSourceCommit.commitId,
                Reviewers = new List<Reviewer>()
            };

            if (x.reviewers != null)
            {
                foreach (TFSAdvanced.Updater.Models.PullRequests.Reviewer reviewer in x.reviewers)
                {
                    if (reviewer.isContainer)
                        continue;

                    // Only ignore the review of the creator if the vote is approved or no response
                    if (reviewer.id == x.createdBy.id && (reviewer.vote == (int)Vote.Approved || reviewer.vote == (int)Vote.NoResponse))
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
    }
}