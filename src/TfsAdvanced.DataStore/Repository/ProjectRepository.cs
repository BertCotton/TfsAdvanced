﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using TfsAdvanced.Models.Projects;
using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.DataStore.Repository
{
    public class ProjectRepository : RepositoryBase<Project>
    {

        public Project GetProject(string projectId)
        {
            return base.Get(p => p.id == projectId);
        }

        protected override int GetId(Project item)
        {
            return item.id.GetHashCode();
        }
    }

    class ProjectComparer : IEqualityComparer<Project>
    {
        public bool Equals(Project x, Project y)
        {
            return x.id == y.id;
        }

        public int GetHashCode(Project obj)
        {
            return obj.id.GetHashCode();
        }
    }
}
