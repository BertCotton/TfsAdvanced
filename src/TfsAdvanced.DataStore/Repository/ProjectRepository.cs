using System;
using Redbus.Interfaces;
using TFSAdvanced.DataStore.Messages;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class ProjectRepository : RepositoryBase<Project, ProjectUpdateMessage>
    {
        public ProjectRepository(IServiceProvider serviceProvider, IEventBus eventBus) : base(serviceProvider, eventBus)
        {
        }

        public Project GetProject(string projectId)
        {
            return base.Get(p => p.Id == projectId);
        }

        protected override int GetId(Project item)
        {
            return item.Id.GetHashCode();
        }
    }
}