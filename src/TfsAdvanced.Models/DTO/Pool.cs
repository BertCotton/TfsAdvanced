using System;
using TFSAdvanced.Models.Interfaces;

namespace TFSAdvanced.Models.DTO
{
    public class Pool : IIdentity, IUpdateTracked
    {
        public int size { get; set; }
        public DateTime createdOn { get; set; }
        public bool autoProvision { get; set; }
        public bool isHosted { get; set; }
        public int Id { get; set; }
        public string name { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
