using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace TfsAdvanced
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    IHostingEnvironment env = builderContext.HostingEnvironment;
                    var appSettingsLocation = Environment.GetEnvironmentVariable("APPSETTING_LOCATION") ?? (env.ContentRootPath);
                    string siteName = Environment.GetEnvironmentVariable("SiteName") ?? "ius";
                    config.AddJsonFile(env.ContentRootPath + "\\appsettings.json")
                                     .AddJsonFile($"{appSettingsLocation}\\appsettings.{siteName}.json", optional: true)
                                     .AddEnvironmentVariables();
                })
                .Build();

            host.Run();
        }
    }
}