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
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Received a new time.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            if (string.IsNullOrEmpty(todo?.employeeId.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a EmployeeId."

                });
            }

            if (string.IsNullOrEmpty(todo?.type.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a Type."

                });
            }

            employeesEntities employeeEntity = new employeesEntities
            {
                employeeId = todo.employeeId,
                type = todo.type,
                isConsolidated = false,
                PartitionKey = "TIME",
                RowKey = Guid.NewGuid().ToString(),
                timestamp  = DateTime.UtcNow,
                ETag = "*"
            };

            TableOperation addOperation = TableOperation.Insert(employeeEntity);
            await todoTable.ExecuteAsync(addOperation);

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
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            string employeeId,
            ILogger log)
        {
            log.LogInformation($"Update for employee: {employeeId}, received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            // Validate todo id
            TableOperation findOperation = TableOperation.Retrieve<employeesEntities>("TIME", employeeId);
            TableResult findResult = await todoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Todo not found."

                });
            }

            // Update todo 
            employeesEntities employeeEntity = (employeesEntities)findResult.Result;
            employeeEntity.isConsolidated = todo.isConsolidated;

            if (!string.IsNullOrEmpty(todo.type.ToString()))
            {
                employeeEntity.type = todo.type;
            }

            TableOperation addOperation = TableOperation.Replace(employeeEntity);
            await todoTable.ExecuteAsync(addOperation);

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
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Get all times received.");

            TableQuery<employeesEntities> query = new TableQuery<employeesEntities>();
            TableQuerySegment<employeesEntities> todos = await todoTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all times.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = todos
            });
        }

        [FunctionName(nameof(GetTimeByID))]
        public static IActionResult GetTimeByID(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "time/{employeeId}")] HttpRequest req,
            [Table("time", "TIME", "{employeeId}", Connection = "AzureWebJobsStorage")] employeesEntities employeeEntity,
            string employeeId,
            ILogger log)
        {
            log.LogInformation($"Get todo by id: {employeeId} received.");

            if (employeeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Todo not found."

                });
            }

            string message = $"Todo {employeeEntity.RowKey}, retrieved.";
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
            [Table("time", "TIME", "{employeeId}", Connection = "AzureWebJobsStorage")] employeesEntities employeeEntity,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            string employeeId,
            ILogger log)
        {
            log.LogInformation($"Delete todo: {employeeId} received.");

            if (employeeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Employee not found."

                });
            }

            await todoTable.ExecuteAsync(TableOperation.Delete(employeeEntity));

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
