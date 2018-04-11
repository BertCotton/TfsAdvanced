using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Web.SocketConnections
{
    public enum ResponseType
    {
        Heartbeat,
        UpdatedCurrentUserPullRequest,
        UpdatedPullRequest,
        NewPullRequest,
        CurrentUserCompletedPullRequest,
        NewCurrentUserCompletedPullRequest
    }
}