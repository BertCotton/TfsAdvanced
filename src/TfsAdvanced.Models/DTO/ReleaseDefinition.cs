using System;
using TFSAdvanced.Models.Interfaces;

namespace TFSAdvanced.Models.DTO
{
    public class ReleaseDefinition : IIdentity, IUpdateTracked
    {
        public int ReleaseDefinitionId { get; set; }

        public string Name { get; set; }

        public int ProjectId { get; set; }

        public Project Project { get; set; }

        public Repository SourceRepository { get; set; }

        public Build Build { get; set; }

        public DateTime LastUpdated { get; set; }
        public int Id { get; set; }
    }
}
