using System;
using Newtonsoft.Json;
using TFSAdvanced.Models.Interfaces;

namespace TFSAdvanced.Models.DTO
{
    public class Reviewer : IIdentity, IUpdateTracked
    {
        public User User { get; set; }

        public int UserId { get; set; }

        [JsonIgnore]
        public PullRequest PullRequest { get; set; }

        public int PullRequestId { get; set; }

        public ReviewStatus ReviewStatus { get; set; }
        public int Id { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
