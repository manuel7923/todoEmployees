using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace todoEmployees.Functions.Entities
{
    public class employeesEntities : TableEntity
    {
        public DateTime timestamp { get; set; }
        public int employeeId { get; set; }
        public string date { get; set; }
        public int type { get; set; }
        public bool isConsolidated { get; set; }
    }
}
