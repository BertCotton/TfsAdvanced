using System;
using System.Collections.Generic;
using System.Text;
using TfsAdvanced.Models;

namespace TFSAdvanced.Updater.Models.Releases
{
    public class ReleaseLink
    {
        public HrefLink self { get; set; }

        public HrefLink web { get; set; }
    }
}
