using System;
using System.IO;
using System.Reflection;
using Swaggerator.Models;

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
            var application = @"\\psf\home\source\mobileapi_cd\Application\";
            var binPath = Path.Combine(application, "Nordstrom.ExternalServices\\bin");
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
                        Console.WriteLine("AssemblyResolve: FOUND");
                        return Assembly.Load(File.ReadAllBytes(Path.Combine(binPath, name + ".dll")));
                    }
                    Console.WriteLine("AssemblyResolve: FileNotFound");
                }
                Console.WriteLine("AssemblyResolve: MISS");
                return null;
            };

            var discoverator = new Discoverator();
            //			var serviceList = discoverator.GetServiceList();
            var serviceList = GetServiceList();
            var baseUri = new Uri(_info.BaseUri, UriKind.Absolute);
            foreach (var service in serviceList.apis)
            {
                var path = service.path;
                if (path.StartsWith("/"))
                {
                    path = path.Substring(1);
                }
                var serviceJson = discoverator.GetServiceDetails(service.serviceType, baseUri, path);
                path = path.Replace('/', '-');
                File.WriteAllText(_groupName + "-" + path + ".json", ReadStream(serviceJson));
            }
            var servicesJson = discoverator.GetServices();
            File.WriteAllText(_groupName + ".json", ReadStream(servicesJson));
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