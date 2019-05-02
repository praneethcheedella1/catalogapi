using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace catalog_api
{
    public static class DeleteAPI
    {
        [FunctionName("DeleteAPI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "delete", Route = "deleteapi/{id}")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "connString")]
                DocumentClient client,
            ILogger log,
            string id)
        {
            // string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("api-catalog", "apicatalog");
            FeedOptions queryOptions = new FeedOptions {  EnableCrossPartitionQuery = true };

            var query = new SqlQuerySpec("SELECT * FROM catalog WHERE catalog.id = @Id", 
            new SqlParameterCollection(new SqlParameter[] { new SqlParameter { Name = "@Id", Value = id }}));
            Document document = client.CreateDocumentQuery(collectionUri, query, queryOptions).AsEnumerable().FirstOrDefault();

            // Document document = client.CreateDocumentQuery(collectionUri, queryOptions).Where(t => t.Id == id).AsEnumerable().FirstOrDefault();

            if (document == null)
            {
                return new NotFoundResult();
            }
            document.SetPropertyValue("deprecated", true);

            // API saveObject = (dynamic)document;
            await client.ReplaceDocumentAsync(document.SelfLink, document);
            return new OkObjectResult(document);



            // log.LogInformation("C# HTTP trigger function processed a request.");

            // FeedOptions queryOptions = new FeedOptions {  EnableCrossPartitionQuery = true, 
            // //PartitionKey = new PartitionKey("catalog") 
            // };
            // string EndpointUri = "https://onemarketplace-apicatalog-db.documents.azure.com:443/";
            // string PrimaryKey = Environment.GetEnvironmentVariable("PrimaryKey");
            // string databaseName = "api-catalog";
            // string collectionName = "apicatalog";
            // DocumentClient client;
            // client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

            // Uri collectionUri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);
            // var document = client.CreateDocumentQuery(collectionUri, queryOptions).Where(t => t.Id == id)
            // .AsEnumerable().FirstOrDefault();
            // //var document = client.CreateDocumentQuery(
            // //    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
            // //   "SELECT * FROM c WHERE c.id = '" + id + "'", 
            // //    queryOptions).AsEnumerable().FirstOrDefault();
            // //var a = document.ToList();

            // if (document == null){
            //     return new NotFoundResult();
            // }

            // await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, id), 
            //     new RequestOptions() { PartitionKey = new PartitionKey(Undefined.Value) });
            // return new OkResult();
        }
    }
}
