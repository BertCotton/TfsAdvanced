using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aggregator.Messages;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using TfsAdvanced.Models;
using TFSAdvanced.Models.DTO;
using PullRequest = TFSAdvanced.Models.DTO.PullRequest;
using RawPullRequest = TFSAdvanced.Aggregator.Raw.Models.PullRequests.PullRequest;
using RawVote = TFSAdvanced.Aggregator.Raw.Models.PullRequests.Vote;

namespace TFSAdvancedAggregator.Actors
{
    public class PullRequestUpdaterActor : TFSBroadcastActorBase<PullRequestUpdaterWorkerActor, PullRequest>
    {
        private IList<PullRequest> PullRequests;
        public PullRequestUpdaterActor(ILogger<PullRequestUpdaterActor> logger) : base(logger, 10, nameof(RepositoryUpdatedMesssage), nameof(BuildUpdatedMessage))
        {
            PullRequests = new List<PullRequest>();
        }
        
    }
    
    

    public class PullRequestUpdaterWorkerActor : TFSActorBase
    {
        private readonly RequestData requestData;

        public PullRequestUpdaterWorkerActor(RequestData requestData, ILogger<PullRequestUpdaterWorkerActor> logger) : base(logger)
        {
            this.requestData = requestData;
            ReceiveAsync<RepositoryUpdatedMesssage>(message => HandleMessageAsync(message, HandleRepositoryUpdatedMessage));
            
            Receive<PullRequestLightMessage>(message => HandleMessage(message, HandlePullRequestLightMessage));
        }

        private bool HandlePullRequestLightMessage(PullRequestLightMessage message)
        {

            TFSAdvanced.Aggregator.Raw.Models.Repositories.CommitLink commitId = null;
            if (message.RawPullRequest.lastMergeCommit != null)
                commitId = message.RawPullRequest.lastMergeCommit;
            else if (message.RawPullRequest.lastMergeSourceCommit != null)
                commitId = message.RawPullRequest.lastMergeSourceCommit;

            if (commitId == null)
            {
                logger.LogWarning($"Unable to get last merge commit for the pullrequest ({message.RawPullRequest.pullRequestId}) {message.RawPullRequest.description}");
                return true;
            }

            if (string.IsNullOrEmpty(commitId.commitId))
            {
                logger.LogWarning($"Unable to get the last commitID for the pull request ({message.RawPullRequest.pullRequestId}) {message.RawPullRequest.description}");
                return true;
            }

            PullRequest pullRequestDto = BuildPullRequest(message.RawPullRequest);
            pullRequestDto.Repository = message.Repository;
            pullRequestDto.RequiredReviewers = message.Repository.MinimumApproverCount;

            foreach (var reviewer in message.RawPullRequest.reviewers)
            {
                // Container reviewers do not count
                if (reviewer.isContainer)
                    continue;
                if (reviewer.vote == (int) RawVote.Approved)
                    pullRequestDto.AcceptedReviewers++;
            }

            pullRequestDto.LastUpdated = DateTime.Now;
            Sender.Tell(pullRequestDto, Self);
            
            LogInformation($"Updated Pullrequest {pullRequestDto.Repository} for {pullRequestDto.Repository.Name} in project {pullRequestDto.Repository.Project.Name}");
            return true;
        }
        
        private PullRequest BuildPullRequest(RawPullRequest x)
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
                foreach (var reviewer in x.reviewers)
                {
                    if (reviewer.isContainer)
                        continue;

                    // Only ignore the review of the creator if the vote is approved or no response
                    if (reviewer.id == x.createdBy.id && (reviewer.vote == (int)RawVote.Approved || reviewer.vote == (int)RawVote.NoResponse))
                        continue;
                    var reviewerDto = new Reviewer
                    {
                        Name = reviewer.displayName,
                        IconUrl = reviewer.imageUrl
                    };
                    switch ((RawVote)reviewer.vote)
                    {
                        case RawVote.Approved:
                            reviewerDto.ReviewStatus = ReviewStatus.Approved;
                            break;

                        case RawVote.ApprovedWithSuggestions:
                            reviewerDto.ReviewStatus = ReviewStatus.ApprovedWithSuggestions;
                            break;

                        case RawVote.NoResponse:
                            reviewerDto.ReviewStatus = ReviewStatus.NoResponse;
                            break;

                        case RawVote.Rejected:
                            reviewerDto.ReviewStatus = ReviewStatus.Rejected;
                            break;

                        case RawVote.WaitingForAuthor:
                            reviewerDto.ReviewStatus = ReviewStatus.WaitingForAuthor;
                            break;
                    }
                    pullRequestDto.Reviewers.Add(reviewerDto);
                }
            }

            // TODO: Link builds

            return pullRequestDto;
        }

        private async Task HandleRepositoryUpdatedMessage(RepositoryUpdatedMesssage message)
        {
            LogInformation($"Fetching PRs for {message.Repository.Project.Name}/{message.Repository.Name}");
            var pullRequests = await GetAsync.FetchResponseList<RawPullRequest>(requestData, message.Repository.PullRequestUrl, logger);
            if(pullRequests == null || !pullRequests.Any())
                LogDebug($"No pull requests found for repostiory {message.Repository.Name} in {message.Repository.Project.Name}");
            LogDebug($"Found {pullRequests.Count} PRs for {message.Repository.Project.Name}/{message.Repository.Name}");
            foreach (var pullRequest in pullRequests)
            {
                Self.Tell(new PullRequestLightMessage(message.Repository, message.Repository.Project, pullRequest), Sender);
            }
        }
    }

    internal class PullRequestLightMessage : MessageBase
    {
        public Repository Repository { get; set; }
        
        public Project Project { get; set; }
        
        public RawPullRequest RawPullRequest { get; set; }

        public PullRequestLightMessage(Repository repository, Project project, RawPullRequest rawPullRequest)
        {
            Repository = repository;
            Project = project;
            RawPullRequest = rawPullRequest;
        }
    }
}