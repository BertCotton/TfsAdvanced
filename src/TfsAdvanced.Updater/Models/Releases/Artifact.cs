using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Updater.Models.Releases
{
    public class Artifact
    {
        public string sourceId { get; set; }

        public string type { get; set; }

        public string alias { get; set; }

        public DefinitionReference definitionReference { get; set; }

        public bool isPrimary { get; set; }
    }
}
