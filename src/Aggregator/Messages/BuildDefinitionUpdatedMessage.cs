using TFSAdvanced.Models.DTO;

namespace Aggregator.Messages
{
    public class BuildDefinitionUpdatedMessage : MessageBase
    {
        public BuildDefinitionUpdatedMessage(BuildDefinition buildDefinition)
        {
            BuildDefinition = buildDefinition;
        }

        public BuildDefinition BuildDefinition { get; set; }
        
        
    }
}