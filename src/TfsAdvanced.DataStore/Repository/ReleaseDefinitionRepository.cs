using System;
using System.Collections.Generic;
using System.Text;
using TFSAdvanced.Models.DTO;

namespace TFSAdvanced.DataStore.Repository
{
    public class ReleaseDefinitionRepository : RepositoryBase<ReleaseDefinition>
    {
        protected override int GetId(ReleaseDefinition item)
        {
            return item.Id;
        }

        public ReleaseDefinition GetReleaseDefinition(int id)
        {
            return Get(definition => definition.Id == id);
        }
    }
}
