using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Web.SocketConnections
{
    public enum ResponseType
    {
        UpdatedCurrentUserPullRequest,
        UpdatedPullRequest,
        NewPullRequest,
        CurrentUserCompletedPullRequest
    }
}
