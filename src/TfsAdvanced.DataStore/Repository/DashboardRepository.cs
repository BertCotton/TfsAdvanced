using System;
using System.Collections.Generic;
using System.Text;
using TFSAdvanced.Models.DTO;

namespace TFSAdvanced.DataStore.Repository
{
    public class DashboardRepository : RepositoryBase<Dashboard>
    {
        protected override int GetId(Dashboard item)
        {
            return item.Id;
        }
    }
}