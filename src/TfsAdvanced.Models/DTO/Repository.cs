using System;
using TFSAdvanced.Models.Interfaces;

namespace TFSAdvanced.Models.DTO
{
    public class Repository : IIdentity, IUpdateTracked
    {

        public string RepositoryId { get; set; }
        public string Name { get; set; }

        public string Url { get; set; }

        public Project Project { get; set; }

        public string PullRequestUrl { get; set; }

        public int MinimumApproverCount { get; set; }
        public int Id { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
