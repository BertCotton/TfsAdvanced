namespace TFSAdvanced.Aggregator.Raw.Models.PullRequests
{
    public enum Vote
    {
        NoResponse = 0,
        Approved = 10,
        ApprovedWithSuggestions = 5,
        WaitingForAuthor = -5,
        Rejected = -10
    }
}