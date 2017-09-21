using System;
using System.Collections.Generic;
using System.Text;
using TFSAdvanced.Models.Interfaces;

namespace TFSAdvanced.Models.DTO
{
    public class Release : IIdentity, IUpdateTracked
    {
        public int Id { get; set; }
        public DateTime LastUpdated { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Reason { get; set; }

        public User CreatedBy { get; set; }

        public ReleaseDefinition ReleaseDefinition { get; set; }

        public Project Project { get; set; }

        public DateTime Created { get; set; }
    }
}
