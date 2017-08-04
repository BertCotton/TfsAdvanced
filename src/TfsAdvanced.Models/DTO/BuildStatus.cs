﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Models.DTO
{
    public enum BuildStatus
    {
        NoBuild,
        NotStarted,
        Building,
        Failed,
        Succeeded,
        Abandonded,
        Cancelled,
        Expired
    }
}