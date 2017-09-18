using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Models.Interfaces
{
    public interface IUpdateTracked
    {
        DateTime LastUpdated { get; set; }
    }
}
