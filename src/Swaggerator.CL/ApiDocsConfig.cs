using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Swaggerator.CL
{
    [DataContract]
    public class ApiDocsConfig
    {
        [DataMember(Name="groups")]
        public IDictionary<string, ServiceGroupInfo> Groups { get; set; } 
    }

    [Serializable]
    [DataContract]
    public class ServiceGroupInfo
    {
        [DataMember(Name = "binPath")]
        public string BinPath { get; set; }

        [DataMember(Name = "baseUri")]
        public string BaseUri { get; set; }

        [DataMember(Name = "services")]
        public Dictionary<string, ServiceInfo> Services { get; set; }

        [DataMember(Name="disabled")]
        public bool Disabled { get; set; }
    }

    [Serializable]
    [DataContract]
    public class ServiceInfo
    {
        [DataMember(Name="serviceClass")]
        public string ServiceClass { get; set; }

        [DataMember(Name="description")]
        public string Description { get; set; }

        [DataMember(Name="disabled")]
        public bool Disabled { get; set; }
    }
}
