using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace todoEmployees.Functions.Entities
{
    public class ConsolidateEntity : TableEntity
    {
        public int EmployeeId { get; set; }

        public DateTime Date { get; set; }

        public double MinutesWork { get; set; }
    }

}
