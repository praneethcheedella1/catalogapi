using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace catalog_api
{
    public static class CreateAPI
    {
        [FunctionName("CreateAPI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "api-catalog",
                collectionName : "apicatalog",
                ConnectionStringSetting = "connString")]
                IAsyncCollector<object> dataOut,
            ILogger log)
        {
                log.LogInformation("Creating a new project");
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                dynamic postbody = JsonConvert.DeserializeObject<object>(requestBody);

                //make guid for api
                postbody.id = Guid.NewGuid();
                
                if (postbody != null){
                   await dataOut.AddAsync(postbody);
                   return (ActionResult)new CreatedResult(postbody.id.ToString(), postbody);
                }
                else {
                    return new BadRequestObjectResult("Please pass a name on the query string or in the request body");
                }
        }
    }
}
