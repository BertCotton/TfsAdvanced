using TfsAdvanced.Models;

namespace TFSAdvanced.Updater.Models.Builds
{
    public class BuildLinks
    {
        public HrefLink web { get; set; }
        public HrefLink timeline { get; set; }
        public HrefLink self { get; set; }
    }
}
