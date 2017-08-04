using System;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace TfsAdvanced.Web
{
    public class HangfireJobFilter : JobFilterAttribute, IApplyStateFilter
    {
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromMinutes(10);
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromHours(10);
        }
    }
}
