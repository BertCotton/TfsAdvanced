using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using TfsAdvanced.DataStore.Repository;
using TFSAdvanced.DataStore.Interfaces;
using TFSAdvanced.Models.DTO;

namespace TFSAdvanced.DataStore.Repository
{
    public class CompletedPullRequestRepository: PullRequestRepository
    {
        public CompletedPullRequestRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public IList<PullRequest> GetForUser(string uniqueName)
        {
            return base.GetList(request => request.ClosedDate.HasValue && request.Creator.UniqueName == uniqueName);
        }
    }
}
