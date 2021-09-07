using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using todoEmployees.Common.Models;
using todoEmployees.Common.Responses;
using todoEmployees.Functions.Entities;

namespace todoEmployees.Functions.Functions
{
    public static class employeesAPI
    {
        [FunctionName(nameof(CreateTime))]
        public static async Task<IActionResult> CreateTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "time")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            ILogger log)
        {
            log.LogInformation("Received a new time.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Time time = JsonConvert.DeserializeObject<Time>(requestBody);

            if (string.IsNullOrEmpty(time?.EmployeeId.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a EmployeeId."

                });
            }

            if (string.IsNullOrEmpty(time?.Type.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a Type."

                });
            }

            EmployeesEntities employeeEntity = new EmployeesEntities
            {
                EmployeeId = time.EmployeeId,
                Type = time.Type,
                Date = time.Date,
                IsConsolidated = false,
                PartitionKey = "TIME",
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*"
            };

            TableOperation addOperation = TableOperation.Insert(employeeEntity);
            await timeTable.ExecuteAsync(addOperation);

            string message = "New time stored in table.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employeeEntity
            });
        }

        [FunctionName(nameof(UpdateTime))]
        public static async Task<IActionResult> UpdateTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "time/{employeeId}")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            string employeeId,
            ILogger log)
        {
            log.LogInformation($"Update for employee: {employeeId}, received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Time time = JsonConvert.DeserializeObject<Time>(requestBody);

            // Validate time id
            TableOperation findOperation = TableOperation.Retrieve<EmployeesEntities>("TIME", employeeId);
            TableResult findResult = await timeTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Time not found."

                });
            }

            // Update time 
            EmployeesEntities employeeEntity = (EmployeesEntities)findResult.Result;
            employeeEntity.IsConsolidated = time.IsConsolidated;

            if (!string.IsNullOrEmpty(time.Type.ToString()))
            {
                employeeEntity.Type = time.Type;
            }

            if (!string.IsNullOrEmpty(time.EmployeeId.ToString()))
            {
                employeeEntity.EmployeeId = time.EmployeeId;
            }

            employeeEntity.Date = time.Date;

            TableOperation addOperation = TableOperation.Replace(employeeEntity);
            await timeTable.ExecuteAsync(addOperation);

            string message = $"Employee: {employeeId}, updated in table.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employeeEntity
            });
        }

        [FunctionName(nameof(GetAllTimes))]
        public static async Task<IActionResult> GetAllTimes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "time")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            ILogger log)
        {
            log.LogInformation("Get all times received.");

            TableQuery<EmployeesEntities> query = new TableQuery<EmployeesEntities>();
            TableQuerySegment<EmployeesEntities> times = await timeTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all times.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = times
            });
        }

        [FunctionName(nameof(GetTimeByID))]
        public static IActionResult GetTimeByID(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "time/{employeeId}")] HttpRequest req,
            [Table("time", "TIME", "{employeeId}", Connection = "AzureWebJobsStorage")] EmployeesEntities employeeEntity,
            string employeeId,
            ILogger log)
        {
            log.LogInformation($"Get time by id: {employeeId} received.");

            if (employeeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Time not found."

                });
            }

            string message = $"Time {employeeEntity.RowKey}, retrieved.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employeeEntity
            });
        }

        [FunctionName(nameof(DeleteTime))]
        public static async Task<IActionResult> DeleteTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "time/{employeeId}")] HttpRequest req,
            [Table("time", "TIME", "{employeeId}", Connection = "AzureWebJobsStorage")] EmployeesEntities employeeEntity,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            string employeeId,
            ILogger log)
        {
            log.LogInformation($"Delete time: {employeeId} received.");

            if (employeeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Employee not found."

                });
            }

            await timeTable.ExecuteAsync(TableOperation.Delete(employeeEntity));

            string message = $"Employee {employeeEntity.RowKey}, deleted.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employeeEntity
            });
        }

    }
}
