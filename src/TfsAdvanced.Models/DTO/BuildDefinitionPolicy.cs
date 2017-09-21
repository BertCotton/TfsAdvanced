using System;
using TFSAdvanced.Models.Interfaces;

namespace TFSAdvanced.Models.DTO
{
    public class BuildDefinitionPolicy : IIdentity, IUpdateTracked
    {
        public int Id { get; set; }
        public string BuildDefinitionPolicyId { get; set; }
        public int BuildDefinitionId { get; set; }

        public int MinimumApproverCount { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
