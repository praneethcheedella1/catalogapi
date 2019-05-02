using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace catalog_api
{
    public static class GetAPIById
    {
        [FunctionName("GetAPIById")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", Route = "getapibyid/{id}")] HttpRequest req,
            ILogger log,
            string id)
        {
            log.LogInformation("Getting todo item by id");

            try {
                FeedOptions queryOptions = new FeedOptions {  EnableCrossPartitionQuery = true };
                string EndpointUri = "https://onemarketplace-apicatalog-db.documents.azure.com:443/";
                string PrimaryKey = Environment.GetEnvironmentVariable("PrimaryKey");
                string databaseName = "api-catalog";
                string collectionName = "apicatalog";
                DocumentClient client;
                client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

                var query = new SqlQuerySpec("SELECT * FROM catalog WHERE catalog.id = @Id", 
                new SqlParameterCollection(new SqlParameter[] { new SqlParameter { Name = "@Id", Value = id }}));

                var result = client.CreateDocumentQuery<object>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                    query, 
                    queryOptions).AsEnumerable().FirstOrDefault();

                if (result == null){
                    throw (new Exception("No results found"));
                }

                return new OkObjectResult(result);

            } catch (Exception e) {
                log.LogInformation(e.ToString());
                return new NotFoundResult();
            }
        }
    }
}
