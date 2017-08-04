using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Updater.Models.Releases
{
    public class ReleaseDefinition
    {
        
        public int id { get; set; }

        public string source { get; set; }

        public int revision { get; set; }

        public string name { get; set; }

        public User createdBy { get; set; }

        public DateTime createdOn { get; set; }

        public User modifiedBy { get; set; }

        public DateTime modifiedOn { get; set; }

        public Artifact[] artifacts { get; set; }

        public string releaseNameFormat { get; set; }

        public ReleaseLink _links { get; set; }

        public string url { get; set; }
    }
}
