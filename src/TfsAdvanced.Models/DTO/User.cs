using System;
using TFSAdvanced.Models.Interfaces;

namespace TFSAdvanced.Models.DTO
{
    public class User : IIdentity, IUpdateTracked
    {
        public string Name { get; set; }

        public string UniqueName { get; set; }

        public string IconUrl { get; set; }
        public int Id { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
