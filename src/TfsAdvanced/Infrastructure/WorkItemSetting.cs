using TfsAdvanced.Data;
using System.Collections.Generic;

namespace TfsAdvanced.Infrastructure
{
    public class WorkItemSetting
    {
        public List<string> IgnoredFields { get; set; }
        public int MaxPriority { get; set; }
        public List<string> ReadOnlyFields { get; set; }
        public List<UserMapping> UserMappings { get; set; }
        public List<string> ExtensionsFromDescription { get; set; }
    }
}