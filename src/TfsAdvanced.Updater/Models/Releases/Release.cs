using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Updater.Models.Releases
{
    public class Release
    {
        public int id { get; set; }

        public DateTime createdOn { get; set; }

        public DateTime modifiedOn { get; set; }

        public User createdBy { get; set; }

        public ReleaseDefinition releaseDefinition { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string reason { get; set; }

        public ProjectReference projectReference { get; set; }
        
    }
}
