using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Models.DTO
{
    public class BuildDefinition
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public int QueueId { get; set; }

        public Repository Repository { get; set; }

        public string Folder { get; set; }

        public string DefaultBranch { get; set; }
    }
}
