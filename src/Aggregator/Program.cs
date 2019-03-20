using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Serilog;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Exceptions;

namespace Aggregator
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
                    
                    Log.Logger = new LoggerConfiguration()
                            .Enrich.WithExceptionDetails()
                            .Enrich.With(new MachineNameEnricher())
                            .Enrich.FromLogContext()
                            .MinimumLevel.Is(LogEventLevel.Information)
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                            .MinimumLevel.Override("System", LogEventLevel.Error)
                            .ReadFrom.Configuration(config.Build())
                        .CreateLogger();
                })
                .ConfigureLogging((hostingContext, logging) => { logging.AddSerilog(dispose: true); }
                )                
                .Build();

            host.Run();
        }
    }
}