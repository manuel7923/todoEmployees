using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace todoEmployees.Functions.Entities
{
    public class EmployeesEntities : TableEntity
    {
        public DateTime Timestamp { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public int Type { get; set; }
        public bool IsConsolidated { get; set; }
        public double MinutesWork { get; set; }
    }
}
