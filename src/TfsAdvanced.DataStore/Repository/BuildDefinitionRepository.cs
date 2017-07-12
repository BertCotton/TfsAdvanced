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

        public BuildDefinition GetBuildDefinition(int definitionId)
        {
            return base.Get(definition => definition.id == definitionId);
        }


        protected override int GetId(BuildDefinition item)
        {
            return item.id;
        }
    }
    
}
