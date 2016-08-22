using System;
using System.Collections.Generic;

namespace TfsAdvanced.Data
{
    public enum Vote
    {
        NoResponse = 0,
        Approved = 10,
        ApprovedWithSuggestions = 5,
        WaitingForAuthor = -5,
        Rejected = -10
    }

    public class _PullRequestLinks
    {
        public HrefLink createdBy { get; set; }
        public HrefLink repository { get; set; }
        public HrefLink self { get; set; }
        public HrefLink sourceBranch { get; set; }
        public HrefLink sourceCommit { get; set; }
        public HrefLink targetBranch { get; set; }
        public HrefLink targetCommit { get; set; }
        public HrefLink workItems { get; set; }
    }

    public class ChangeCounts
    {
        public int? Add { get; set; }
        public int? Delete { get; set; }
        public int? Edit { get; set; }
    }

    public class Commit
    {
        public Person author { get; set; }
        public ChangeCounts changecounts { get; set; }
        public string comment { get; set; }
        public string commitId { get; set; }
        public Person committer { get; set; }
    }

    public class CommitLink
    {
        public string commitId { get; set; }
        public string url { get; set; }
    }

    public class Createdby
    {
        public string displayName { get; set; }
        public string id { get; set; }
        public string imageUrl { get; set; }
        public string uniqueName { get; set; }
        public string url { get; set; }
    }

    public class CreateRepositoryRequest
    {
        public ProjectGuid id { get; set; }
        public string name { get; set; }
    }

    public class HrefLink
    {
        public string href { get; set; }
    }

    public class Person
    {
        public ChangeCounts churn { get; set; }
        public string date { get; set; }
        public string email { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Project
    {
        public ProjectLinks _links { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string remoteUrl { get; set; }
        public string state { get; set; }
        public string url { get; set; }
    }

    public class ProjectLinks
    {
        public HrefLink commits { get; set; }
        public HrefLink items { get; set; }
        public HrefLink project { get; set; }
        public HrefLink pullRequests { get; set; }
        public HrefLink pushes { get; set; }
        public HrefLink refs { get; set; }
        public HrefLink self { get; set; }
        public HrefLink web { get; set; }
    }

    public class PullRequest
    {
        public _PullRequestLinks _links { get; set; }
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
        public Repository repository { get; set; }
        public Reviewer[] reviewers { get; set; }
        public string sourceRefName { get; set; }
        public string targetRefName { get; set; }
        public string title { get; set; }
        public string url { get; set; }
    }

    public class Repository
    {
        public List<Commit> commits { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public Project project { get; set; }
        public string remoteUrl { get; set; }
        public string url { get; set; }
    }

    public class RepositoryExistsCheck
    {
        public bool exists { get; set; }
        public string name { get; set; }
        public string projectId { get; set; }
        public string projectName { get; set; }
        public string url { get; set; }
    }

    public class Response<T>
    {
        public int count { get; set; }
        public T value { get; set; }
    }

    public class Reviewer
    {
        public string displayName { get; set; }
        public string id { get; set; }
        public string imageUrl { get; set; }
        public bool isContainer { get; set; }
        public string reviewerUrl { get; set; }
        public string uniqueName { get; set; }
        public string url { get; set; }
        public int vote { get; set; }
        public Votedfor[] votedFor { get; set; }
    }

    public class Votedfor
    {
        public string displayName { get; set; }
        public string id { get; set; }
        public string imageUrl { get; set; }
        public bool isContainer { get; set; }
        public string reviewerUrl { get; set; }
        public string uniqueName { get; set; }
        public string url { get; set; }
        public int vote { get; set; }
        public Vote VoteStatus => (Vote)vote;
    }
}