using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using TfsAdvanced.Models.Builds;
using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.DataStore.Repository
{
    public class BuildDefinitionRepository : RepositoryBase<BuildDefinition>
    {
        
        public BuildDefinitionRepository() : base(new BuildDefinitionComparer())
        {
        }

     
        public BuildDefinition GetBuildDefinition(int definitionId)
        {
            return base.Get(() => data.FirstOrDefault(x => x.id == definitionId));
        }

    }

    class BuildDefinitionComparer : IEqualityComparer<BuildDefinition>
    {
        public bool Equals(BuildDefinition x, BuildDefinition y)
        {
            return x.id == y.id;
        }

        public int GetHashCode(BuildDefinition obj)
        {
            // The ID of a buildDefinition should be unique to a hash code
            return obj.id;
        }
    }
}
