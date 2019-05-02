using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

using System.Diagnostics;
using YamlDotNet.RepresentationModel;

namespace catalog_api
{
    public static class GetCatalogAPIHealthcheck
    {
        const string timeFormat = "HH:mm:ss";
        const string infoNodeName = "info";
        const string versionNodeName = "version";
        const string titleNodeName = "title";
        const string yamlFile = "../../../../catalog-api.yaml";

        public struct Version
        {
            public int major { get; set; }
            public int minor { get; set; }
            public int release { get; set; }
        }

        public class HealthCheckResponse
        {
            public int code { get; set; }
            public string name { get; set; }
            public Version version { get; set; }
            public int uptime { get; set; }
            public string lastStarted { get; set; }
        }

        struct ErrorResponse
        {
            public int code { get; set; }
            public string message { get; set; }
        }

        [FunctionName("GetCatalogAPIHealthcheck")]
        public static async Task<String> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", Route = "healthcheck")] 
            HttpRequest req, ILogger log)
        {
            try
            {
                string yamlFilePath = Environment.CurrentDirectory + yamlFile;

                HealthCheckResponse response = GetHealhCheckVersionInfo(yamlFilePath);
                response = GetHealthCheckTimeInfo(response);
                response.code = 200;
                string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                return jsonResponse;
            } catch(Exception e)
            {
                ErrorResponse response = new ErrorResponse
                {
                    code = 500,
                    message = "Error getting healthcheck: " + e.Message
                };
                string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                return jsonResponse;
            }
        }

        public static HealthCheckResponse GetHealhCheckVersionInfo(string filePath)
        {
            HealthCheckResponse response = new HealthCheckResponse();
            Version version = new Version();

            // parse yaml file
            var reader = new StreamReader(filePath);
            var yaml = new YamlStream();
            yaml.Load(reader);

            // holds entire yaml document
            YamlMappingNode mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            // loop through to pull out version and title
            foreach (KeyValuePair<YamlNode, YamlNode> node in mapping.Children)
            {
                // grab info node
                if (infoNodeName.Equals(node.Key.ToString()))
                {
                    YamlMappingNode infoNode = (YamlMappingNode)node.Value;

                    // grab values for info subnodes (version and title)
                    foreach (KeyValuePair<YamlNode, YamlNode> innerNode in infoNode.Children)
                    {
                        if (versionNodeName.Equals(innerNode.Key.ToString()))
                        {
                            version.major = int.Parse(innerNode
                                .Value
                                .ToString()
                                .Split('.')[0]);

                            version.minor = int.Parse(innerNode
                                .Value
                                .ToString()
                                .Split('.')[1]);

                            version.release = int.Parse(innerNode
                                .Value
                                .ToString()
                                .Split('.')[2]);
                        }
                        if (titleNodeName.Equals(innerNode.Key.ToString()))
                        {
                            response.name = innerNode.Value.ToString();
                        }
                    }
                }
            }
            response.version = version;
            return response;
        }

        public static HealthCheckResponse GetHealthCheckTimeInfo(HealthCheckResponse response)
        {
            DateTime now = DateTime.UtcNow;
            DateTime startTime = Process.GetCurrentProcess().StartTime.ToUniversalTime();
            int uptime = (int)Math.Floor((now - startTime).TotalSeconds);

            // format the date string to include day of week and to indicate UTC timezone
            string lastStarted = string.Format("{0} {1} {2}", 
                startTime.ToLongDateString(), startTime.ToString(timeFormat), startTime.Kind);

            response.uptime = uptime;
            response.lastStarted = lastStarted;
            return response;
        }

    }

}