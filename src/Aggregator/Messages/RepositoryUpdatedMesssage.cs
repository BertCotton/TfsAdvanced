using TFSAdvanced.Models.DTO;

namespace Aggregator.Messages
{
    public class RepositoryUpdatedMesssage : MessageBase
    {
        public RepositoryUpdatedMesssage(Repository repository)
        {
            Repository = repository;
        }

        public Repository Repository { get; set; }
        
    }
}