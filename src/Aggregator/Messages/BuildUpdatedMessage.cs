using TFSAdvanced.Models.DTO;

namespace Aggregator.Messages
{
    public class BuildUpdatedMessage : MessageBase
    {
        public BuildUpdatedMessage(Build build)
        {
            Build = build;
        }

        public Build Build { get; set; }
    }
}