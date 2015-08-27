using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Swaggerator.CL
{
    public class MainClass
	{
 		public static void Main (string[] args)
		{
            if (args.Length < 1)
            {
                Console.WriteLine("apiDocsConfigJson required");
                return;
            }

            var json = File.ReadAllText(args[0]);
            var config = JsonConvert.DeserializeObject<ApiDocsConfig>(json);

 		    foreach (var groupName in config.Groups.Keys)
 		    {
 		        var group = config.Groups[groupName];
 		        if (group.Disabled)
 		        {
 		            continue;
 		        }
 		        var appDomain = AppDomain.CreateDomain(groupName + " - GroupSwaggerator");
 		        var aggregatorHandle = appDomain.CreateInstanceFrom(
 		            Assembly.GetExecutingAssembly().Location,
 		            "Swaggerator.CL.GroupSwaggerator",
 		            false,
 		            0,
 		            null,
 		            new object[]
 		            {
 		                groupName,
                        group
 		            },
 		            CultureInfo.CurrentCulture,
 		            null);
 		    }

			Console.WriteLine ("success! " + config.Groups.Sum(x => x.Value.Services.Keys.Count));
		    Console.ReadKey();
		}
	}
}
