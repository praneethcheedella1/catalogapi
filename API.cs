using System.Collections.Generic;

namespace catalog_api
{
    public class API
    {
        public string id { get; set; }
        public string apiName { get; set; }
        public string environment { get; set; }
        public string kongWhiteListGroup { get; set; }
        public string description { get; set; }
        public List<string> contactEmailList { get; set; }
        public bool requiresApproval { get; set; }
        public bool catalogAPI { get; set; }
        public bool featuredAPI { get; set; }
        public string informationURL { get; set; }
        public string authType { get; set; }
        public bool deprecated { get; set; }
        public int versionMajor { get; set; }
        public int versionMinor { get; set; }
        public int versionRevision { get; set; }
    }
}
