using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace TfsAdvanced.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UpdateFieldOperation
    {
        add,
        replace,
        test
    }

    public interface Update
    {
        UpdateFieldOperation op { get; set; }
        string path { get; set; }
    }

    public class Relations
    {
        public Dictionary<string, object> attributes { get; set; }
        public string rel { get; set; }
        public string url { get; set; }
        public WorkItem workItem { get; set; }
    }

    public class UpdateField : Update
    {
        public UpdateFieldOperation op { get; set; }
        public string path { get; set; }
        public string value { get; set; }
    }

    public class UpdateRelation : Update
    {
        public UpdateFieldOperation op { get; set; }
        public string path { get; set; }
        public UpdateRelationValue value { get; set; }
    }

    public class UpdateRelationValue
    {
        public Dictionary<string, object> attributes { get; set; }
        public string rel { get; set; }
        public string url { get; set; }
    }

    public class WorkItem
    {
        public Dictionary<string, string> fields { get; set; }
        public int id { get; set; }
        public Project project { get; set; }
        public Relations[] relations { get; set; }
        public int rev { get; set; }
        public string url { get; set; }
    }
}