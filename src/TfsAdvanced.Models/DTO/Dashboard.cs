using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Models.DTO
{
    public class Dashboard
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ProjectGuid { get; set; }

        public Project Project { get; set; }

        public string ReleaseGuid { get; set; }

        public ReleaseDefinition Release { get; set; }
    }
}