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
    public static class UpdateAPI
    {
        [FunctionName("UpdateAPI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "put", Route = "updateapi/{id}")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "connString")]
                DocumentClient client,
            ILogger log,
            string id)
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updated = JsonConvert.DeserializeObject<API>(requestBody);
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri("api-catalog", "apicatalog");
                FeedOptions queryOptions = new FeedOptions {  EnableCrossPartitionQuery = true };

                var query = new SqlQuerySpec("SELECT * FROM catalog WHERE catalog.id = @Id", 
                new SqlParameterCollection(new SqlParameter[] { new SqlParameter { Name = "@Id", Value = id }}));
                Document document = client.CreateDocumentQuery(collectionUri, query, queryOptions).AsEnumerable().FirstOrDefault();

                // var document = client.CreateDocumentQuery(collectionUri, queryOptions).Where(t => t.Id == id).AsEnumerable().FirstOrDefault();

                if (document == null)
                {
                    return new NotFoundResult();
                }

                await client.ReplaceDocumentAsync(document.SelfLink, updated);
                object res = (dynamic)updated;
                return new OkObjectResult(res);
            }
    }
}
