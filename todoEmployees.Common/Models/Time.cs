﻿using System;

namespace todoEmployees.Common.Models
{
    public class Time
    {
        public int EmployeeId { get; set; }

        public DateTime Date { get; set; }

        public int Type { get; set; }

        public bool IsConsolidated { get; set; }

    }
}
