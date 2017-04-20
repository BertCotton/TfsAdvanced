namespace TfsAdvanced.Models.JobRequests
{
    public enum JobRequestStatus
    {
        queued,
        assigned,
        started,
        succeeded,
        failed,
        abandoned,
        canceled,

    }
}
