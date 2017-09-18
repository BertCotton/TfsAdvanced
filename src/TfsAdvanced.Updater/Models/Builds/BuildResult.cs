namespace TFSAdvanced.Updater.Models.Builds
{
    public enum BuildResult
    {
        failed,
        succeeded,
        partiallySucceeded,
        succeededWithIssues,
        canceled,
        abandoned,
        expired
    }
}