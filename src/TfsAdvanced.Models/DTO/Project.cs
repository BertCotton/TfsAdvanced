using System;
using TFSAdvanced.Models.Interfaces;

namespace TFSAdvanced.Models.DTO
{
    public class Project : IIdentity, IUpdateTracked
    {
        public int Id { get; set; }

        public string ProjectId { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
