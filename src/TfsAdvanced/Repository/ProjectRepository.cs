using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data.Projects;

namespace TfsAdvanced.Repository
{
    public class ProjectRepository
    {
        private ConcurrentBag<Project> projects;

        public ProjectRepository()
        {
            projects = new ConcurrentBag<Project>();
        }

        public IList<Project> GetProjects()
        {
            return projects.ToImmutableList();    
        }

        public Project GetProject(string projectId)
        {
            return projects.FirstOrDefault(p => p.id == projectId);
        }

        public void Update(IList<Project> updatedProjects)
        {
            this.projects = new ConcurrentBag<Project>(updatedProjects);
        }

    }
}
