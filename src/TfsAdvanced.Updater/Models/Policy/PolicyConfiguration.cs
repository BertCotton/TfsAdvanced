namespace TFSAdvanced.Updater.Models.Policy
{
    public class PolicyConfiguration
    {
        public string id { get; set; }
        public PolicySetting settings { get; set; }

        public PolicyType type { get; set; }
    }
}
