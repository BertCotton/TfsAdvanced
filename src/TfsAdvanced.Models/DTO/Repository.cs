using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Models.DTO
{
    public class Repository
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Url { get; set; }

        public Project Project { get; set; }

        public string PullRequestUrl { get; set; }

        public int MinimumApproverCount { get; set; }
    }
}
