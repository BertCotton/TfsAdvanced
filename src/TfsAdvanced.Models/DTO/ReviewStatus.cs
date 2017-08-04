using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Models.DTO
{
    public enum ReviewStatus
    {
        NoResponse,
        Approved,
        ApprovedWithSuggestions,
        WaitingForAuthor,
        Rejected
    }
}
