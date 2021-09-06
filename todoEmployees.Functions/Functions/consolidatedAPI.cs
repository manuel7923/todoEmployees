using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using todoEmployees.Functions.Entities;
using todoEmployees.Common.Responses;
using todoEmployees.Common.Models;
using todoEmployees.Functions.Functions;
using Microsoft.AspNetCore.Http.Internal;
using todoEmployees.Tests.Helpers;
using System.Net;

namespace todoEmployees.Functions.Functions
{
    public static class ConsolidatedAPI
    {

        [FunctionName(nameof(CreateConsolidate))]
        public static async Task<IActionResult> CreateConsolidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consolidate")] HttpRequest req,
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "time")] HttpRequest req,
            [Table("consolidate", Connection = "AzureWebJobsStorage")] CloudTable TodoTableTime,
            ILogger log)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);
            log.LogInformation("hello");

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = "hello",
                Result = todo
            });

        }
    }
}
