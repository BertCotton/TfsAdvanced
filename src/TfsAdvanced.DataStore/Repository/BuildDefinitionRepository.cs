using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class BuildDefinitionRepository : RepositoryBase<BuildDefinition>
    {

        public BuildDefinition GetBuildDefinition(int definitionId)
        {
            return base.Get(definition => definition.Id == definitionId);
        }


        protected override int GetId(BuildDefinition item)
        {
            return item.Id;
        }
    }
    
}
