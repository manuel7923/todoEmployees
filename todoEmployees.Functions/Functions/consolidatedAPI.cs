using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using todoEmployees.Common.Models;
using todoEmployees.Common.Responses;
using todoEmployees.Functions.Entities;

namespace todoEmployees.Functions.Functions
{
    public static class ConsolidatedAPI
    {
        [FunctionName(nameof(GetAllConsolidatesByDate))]
        public static async Task<IActionResult> GetAllConsolidatesByDate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consolidated")] HttpRequest req,
            [Table("consolidate", Connection = "AzureWebJobsStorage")] CloudTable consolidateTable,
            ILogger log)
        {
            log.LogInformation("Get all times received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Consolidate consolidate = JsonConvert.DeserializeObject<Consolidate>(requestBody);

            string filterDate = TableQuery.GenerateFilterConditionForDate("Date", QueryComparisons.Equal, DateTime.Parse(consolidate.Date.ToString()));
            TableQuery<ConsolidateEntity> query = new TableQuery<ConsolidateEntity>().Where(filterDate);
            TableQuerySegment<ConsolidateEntity> consolidateResult = await consolidateTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all consolidates.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = consolidateResult
            });
        }

        [FunctionName(nameof(CreateConsolidate))]
        public static async Task<IActionResult> CreateConsolidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consolidate")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            [Table("consolidate", Connection = "AzureWebJobsStorage")] CloudTable consolidateTable,
            ILogger log)
        {

            log.LogInformation("Get all times received.");

            string filterEmployees = TableQuery.GenerateFilterConditionForBool("IsConsolidated", QueryComparisons.Equal, false);
            TableQuery<EmployeesEntities> query = new TableQuery<EmployeesEntities>().Where(filterEmployees);
            TableQuerySegment<EmployeesEntities> entries = await timeTable.ExecuteQuerySegmentedAsync(query, null);
            List<EmployeesEntities> orderedEntries = entries.OrderBy(x => x.EmployeeId).ThenBy(x => x.Date).ToList();

            if (orderedEntries.Count > 0)
            {
                int totalConsolidated = 0;
                for (var i = 0; i < orderedEntries.Count; i++)
                {
                    if (orderedEntries.Count == (i + 1))
                    {
                        break;
                    }

                    if (orderedEntries[i].Type == 1 && orderedEntries[i + 1].Type == 0)
                    {
                        i++;
                        continue;
                    }

                    if (orderedEntries[i].EmployeeId == orderedEntries[i + 1].EmployeeId) 
                    {
                        string filterById = TableQuery.GenerateFilterConditionForInt("EmployeeId", QueryComparisons.Equal, orderedEntries[i].EmployeeId);
                        TableQuery<ConsolidateEntity> queryEmp = new TableQuery<ConsolidateEntity>().Where(filterById);
                        TableQuerySegment<ConsolidateEntity> Consolidates = await consolidateTable.ExecuteQuerySegmentedAsync(queryEmp, null);

                        totalConsolidated = totalConsolidated + 1;



                        //TODO: MISSING 
                        EmployeesEntities employeeEntity = new EmployeesEntities();

                        TableResult findEmployeeOne = await timeTable.ExecuteAsync(TableOperation.Retrieve<EmployeesEntities>("TIME", orderedEntries[i].RowKey));
                        employeeEntity = (EmployeesEntities)findEmployeeOne.Result;
                        employeeEntity.IsConsolidated = true;
                        await timeTable.ExecuteAsync(TableOperation.Replace(employeeEntity));

                        TableResult findEmployeeTwo = await timeTable.ExecuteAsync(TableOperation.Retrieve<EmployeesEntities>("TIME", orderedEntries[i + 1].RowKey));
                        employeeEntity = (EmployeesEntities)findEmployeeTwo.Result;
                        employeeEntity.IsConsolidated = true;
                        await timeTable.ExecuteAsync(TableOperation.Replace(employeeEntity));

                        double minutes = (orderedEntries[i + 1].Date - orderedEntries[i].Date).TotalMinutes;
                        DateTime DateConsolidate = new DateTime(orderedEntries[i].Date.Year, orderedEntries[i].Date.Month, orderedEntries[i].Date.Day);

                        ConsolidateEntity consolidateEntity = new ConsolidateEntity
                        {
                            EmployeeId = orderedEntries[i].EmployeeId,
                            Date = DateConsolidate,
                            MinutesWork = minutes,
                            PartitionKey = "CONSOLIDATE",
                            RowKey = Guid.NewGuid().ToString(),
                            ETag = "*"
                        };

                        if (Consolidates.Results.Count == 0)
                        {
                            TableOperation addOperationCre = TableOperation.Insert(consolidateEntity);
                            await consolidateTable.ExecuteAsync(addOperationCre);
                        } 
                        else
                        {
                            TableResult findEmployee = await consolidateTable.ExecuteAsync(TableOperation.Retrieve<ConsolidateEntity>("CONSOLIDATE", Consolidates.Results.ElementAt(0).RowKey));
                            consolidateEntity = (ConsolidateEntity)findEmployee.Result;
                            consolidateEntity.MinutesWork = consolidateEntity.MinutesWork + (double)minutes;
                            await consolidateTable.ExecuteAsync(TableOperation.Replace(consolidateEntity));
                        }
                    }
                    i++;
                }

            }

            string message = "Retrieved all times.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = "ok"
            });


        }
    }
}
