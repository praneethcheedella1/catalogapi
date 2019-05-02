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
    public static class UpdatePartialAPI
    {
        [FunctionName("UpdatePartialAPI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "patch", Route = "updatepartialapi/{id}")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "connString")]
                DocumentClient client,
            ILogger log,
            string id)
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                Uri collectionUri = UriFactory.CreateDocumentCollectionUri("api-catalog", "apicatalog");
                FeedOptions queryOptions = new FeedOptions {  EnableCrossPartitionQuery = true };

                var query = new SqlQuerySpec("SELECT * FROM catalog WHERE catalog.id = @Id", 
                new SqlParameterCollection(new SqlParameter[] { new SqlParameter { Name = "@Id", Value = id }}));
                Document document = client.CreateDocumentQuery(collectionUri, query, queryOptions).AsEnumerable().FirstOrDefault();

                //.Where(t => t.Id == id)

                if (document == null)
                {
                    return new NotFoundResult();
                }
                string link = document.SelfLink;

                var bodyProperties = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(requestBody);
                foreach (var parameter in bodyProperties){
                    document.SetPropertyValue(parameter.Key, parameter.Value);
                }

                API saveObject = (dynamic)document;
                await client.ReplaceDocumentAsync(link, saveObject);
                return new OkObjectResult(saveObject);
            }
    }
}
