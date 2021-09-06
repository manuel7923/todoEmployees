using System;
using System.Collections.Generic;
using System.Text;

namespace todoEmployees.Common.Models
{
    public class Todo
    {
        public DateTime timestamp { get; set; }

        public int employeeId { get; set; }

        public DateTime date { get; set; }

        public int type { get; set; }

        public bool isConsolidated { get; set; }

    }
}
