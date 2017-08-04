using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Models.DTO
{
    public class Reviewer : User
    {
        public ReviewStatus ReviewStatus { get; set; }
    }
}
