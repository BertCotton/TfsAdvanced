using TFSAdvanced.Models.DTO;

namespace Aggregator.Messages
{
    public class ProjectUpdatedMessage : MessageBase
    {
        public ProjectUpdatedMessage(Project project)
        {
            this.Project = project;
        }

        public Project Project { get; set; }
    }
}