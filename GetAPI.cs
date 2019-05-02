using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Linq;

namespace catalog_api
{
    public static class GetAPI
    {
        [FunctionName("GetAPI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try {
                FeedOptions queryOptions = new FeedOptions {  EnableCrossPartitionQuery = true };
                string EndpointUri = "https://onemarketplace-apicatalog-db.documents.azure.com:443/";
                string PrimaryKey = Environment.GetEnvironmentVariable("PrimaryKey");
                string databaseName = "api-catalog";
                string collectionName = "apicatalog";
                DocumentClient client;
                client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);
                
                SqlParameterCollection sqlCollection = new SqlParameterCollection();
                string whereString;
                whereString = "";
                var paramsList = req.GetQueryParameterDictionary();
                var first = true;
                int page=0;
                int pageSize=0;
                foreach (var param in paramsList){
                    if (param.Key != "page"){
                        if (param.Key != "pageSize"){
                            if (first){
                                first = false;
                                whereString += " WHERE";
                            }else{
                                whereString += " AND";
                            }
                            whereString += " q." + param.Key + " = @" + param.Key;
                            sqlCollection.Add(new SqlParameter { Name = "@" + param.Key, Value = param.Value });
                        }else{
                            pageSize = Int32.Parse(param.Value);
                        }
                    }else{
                        page = Int32.Parse(param.Value);
                    }
                }

                var query = new SqlQuerySpec("SELECT * FROM catalog as q" + whereString, sqlCollection);

                var result = client.CreateDocumentQuery<object>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                    query, 
                    queryOptions).AsEnumerable().ToList();

                if (page!=0 && pageSize !=0){
                    var pagresult = result.Skip((page-1)*pageSize).Take(pageSize);
                    if (pagresult == null){
                        throw (new Exception("No results found"));
                    }
                    log.LogInformation("Getting list of api's");
                    return new OkObjectResult(pagresult);
                }

                if (result == null){
                    throw (new Exception("No results found"));
                }
                log.LogInformation("Getting list of api's");
                return new OkObjectResult(result);

            } catch (Exception e) {
                return new NotFoundResult();
            }
        }
    }
}