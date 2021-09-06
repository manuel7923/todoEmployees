using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using todoEmployees.Functions.Entities;

namespace todoEmployees.Functions.Functions
{
    public static class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async Task Run(
            [TimerTrigger("0 */2 * * * *")] TimerInfo myTimer,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable consolidateTable,
            ILogger log)
        {
            log.LogInformation($"Deleting completed function executed at: {DateTime.Now}");

            string filter = TableQuery.GenerateFilterConditionForBool("isConsolidated", QueryComparisons.Equal, false);

            TableQuery<employeesEntities> query = new TableQuery<employeesEntities>().Where(filter);
            // This executes the session
            TableQuerySegment<employeesEntities> unConsolidatedTimes = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            // Count Elements
            int deleted = 0;
            foreach (employeesEntities unConsolidatedTime in unConsolidatedTimes)
            {
                await consolidateTable.ExecuteAsync(TableOperation.Insert(unConsolidatedTime));
                deleted++;
            }

            log.LogInformation($"Deleted: {deleted} items at: {DateTime.Now}");

        }
    }
}
