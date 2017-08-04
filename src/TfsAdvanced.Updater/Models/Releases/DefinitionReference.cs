using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Updater.Models.Releases
{
    public class DefinitionReference
    {
        public IdNameReference artifactSourceDefinitionUrl { get; set; }

        public IdNameReference definition { get; set; }

        public IdNameReference project { get; set; }

        public IdNameReference defaultVersionType { get; set; }
    }
}
