using TfsAdvanced.Models;

namespace TFSAdvanced.Updater.Models.Builds
{
    public class BuildDefinitionLinks
    {
        public HrefLink self { get; set; }

        public HrefLink web { get; set; }

        public HrefLink badge { get; set; }
    }
}
