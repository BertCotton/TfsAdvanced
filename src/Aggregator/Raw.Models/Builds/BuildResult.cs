namespace TFSAdvanced.Aggregator.Raw.Models.Builds
{
    public enum BuildResult
    {
        failed,
        succeeded,
        partiallySucceeded,
        canceled,
        abandoned,
        expired
    }
}