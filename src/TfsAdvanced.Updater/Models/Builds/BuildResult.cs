namespace TFSAdvanced.Updater.Models.Builds
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