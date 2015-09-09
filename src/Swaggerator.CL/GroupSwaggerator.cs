using System;
using System.IO;
using System.Reflection;
using Swaggerator.Models;
using Newtonsoft.Json;

namespace Swaggerator.CL
{
    public class GroupSwaggerator
    {
        private readonly string _groupName;
        private readonly ServiceGroupInfo _info;

        public GroupSwaggerator(string groupName, ServiceGroupInfo info)
        {
            _groupName = groupName;
            _info = info;
            Load();
        }

        public void Load()
        {
            var application = @"../../../../../mobileapi_cd/Application/";
            var binPath = Path.Combine(application, "Nordstrom.ExternalServices/bin");
            AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs rea) =>
            {
                Console.WriteLine("AssemblyResolve: " + rea.Name);
                var parts = rea.Name.Split(new[] { ',' }, StringSplitOptions.None);
                var name = parts[0];
                if (name.StartsWith("Nordstrom") || name.Equals("Microsoft.ServiceModel.Web"))
                {
                    var file = Path.Combine(binPath, name + ".dll");
                    if (File.Exists(file))
                    {
                        Console.WriteLine("AssemblyResolve: FOUND: " + file);
                        return Assembly.Load(File.ReadAllBytes(Path.Combine(binPath, name + ".dll")));
                    }
                    Console.WriteLine("AssemblyResolve: FileNotFound: " + file);
                }
                Console.WriteLine("AssemblyResolve: MISS");
                return null;
            };

            var discoverator = new Discoverator();
            var serviceList = GetServiceList();
            var baseUri = new Uri(_info.BaseUri, UriKind.Absolute);
            if (Directory.Exists(_groupName))
            {
                Directory.Delete(_groupName, true);
            }
            Directory.CreateDirectory(_groupName);
            foreach (var service in serviceList.apis)
            {
                var servicePath = service.path;
                if (servicePath.StartsWith("/"))
                {
                    servicePath = servicePath.Substring(1);
                }
                var serviceJson = discoverator.GetServiceDetails(service.serviceType, baseUri, servicePath);
                var servicePathParts = servicePath.Split('/');
                for (var p = 0; p < (servicePathParts.Length - 1); p++)
                {
                    var pathParts = new string[p+3];
                    pathParts[0] = _groupName;
                    Array.Copy(servicePathParts, 0, pathParts, 1, p+1);
                    var subPath = string.Join("/", pathParts);
                    if (!Directory.Exists(subPath))
                    {
                        Directory.CreateDirectory(subPath);
                    }
                }
                File.WriteAllText(Path.Combine(_groupName, servicePath + ".swagger"), ReadStream(serviceJson));
            }
            //var servicesJson = discoverator.GetServices();
            var servicesJson = JsonConvert.SerializeObject(serviceList);
            File.WriteAllText(_groupName + ".swagger", servicesJson);
        }

        private static string ReadStream(Stream s)
        {
            using (var reader = new StreamReader(s))
            {
                return reader.ReadToEnd();
            }
        }

        private ServiceList GetServiceList()
        {
            var result = new ServiceList();
            foreach (var serviceName in _info.Services.Keys)
            {
                var serviceInfo = _info.Services[serviceName];
                if (serviceInfo.Disabled)
                {
                    continue;
                }
                var service = GetService(serviceName, serviceInfo);
                result.apis.Add(service);
            }
            return result;
        }

        private static Service GetService(string servicePath, ServiceInfo serviceInfo)
        {
            var service = new Service
            {
                description = serviceInfo.Description,
                path = servicePath,
                serviceType = Type.GetType(serviceInfo.ServiceClass)
            };
            return service;
        }

        private static Swaggerator.Models.Service GetService(string path, string serviceTypeName, string description)
        {
            return new Swaggerator.Models.Service
            {
                path = path,
                description = description,
                serviceType = Type.GetType(serviceTypeName)
            };
        }
    }
}