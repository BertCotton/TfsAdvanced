using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Models.DTO
{
    public class Build
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Folder { get; set; }

        public string Url { get; set; }

        public User Creator { get; set; }

        public Repository Repository { get; set; }

        public string SourceCommit { get; set; }

        public BuildStatus BuildStatus { get; set; }

        public DateTime QueuedDate { get; set; }

        public DateTime? StartedDate { get; set; }

        public DateTime? FinishedDate { get; set; }
    }
}
