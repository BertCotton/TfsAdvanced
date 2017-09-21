using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using TfsAdvanced.Data;
using TfsAdvanced.Infrastructure;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
using TfsAdvanced.ServiceRequests;
using TfsAdvanced.Web;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Exceptions;
using TfsAdvanced.Web.SocketConnections;
using TFSAdvanced.DataStore;
using TFSAdvanced.DataStore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace TfsAdvanced
{
    public class Startup
    {
        public static readonly int MAX_DEGREE_OF_PARALLELISM = -1;
        private readonly string siteName = Environment.GetEnvironmentVariable("SiteName") ?? "ius";
        public IConfigurationRoot Configuration { get; set; }

        public IList<Type> References;

        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{siteName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            References = new List<Type>
            {
                typeof(IdentityRole),
                typeof(Options)
            };
            
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            

            services.AddEntityFrameworkInMemoryDatabase().AddDbContext<TfsAdvancedInMemoryDataContext>();
            services.AddEntityFrameworkSqlServer().AddDbContext<TfsAdvancedSqlDataContext>();

            services.AddSingleton<IConfiguration>(Configuration);

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<TfsAdvancedInMemoryDataContext>()
                .AddDefaultTokenProviders();

            services.AddSession(options =>
            {
                options.CookieHttpOnly = true;
                options.CookieName = "TFSAdvanced";
                options.IdleTimeout = TimeSpan.FromHours(5);
            });

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            }).AddMvcOptions(options => options.Filters.Add(new ExceptionHandler()));
            services.AddHangfire(configuration =>
            {
                configuration.UseStorage(new MemoryStorage());
            });

            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            var builder = new ContainerBuilder();

            builder.Populate(services);

            builder.RegisterType<AuthenticationTokenProvider>();


            builder.RegisterType<SignInManager<ApplicationUser>>().AsSelf();

            builder.RegisterAssemblyTypes(Assembly.Load(Assembly.GetEntryAssembly().GetReferencedAssemblies().First(t => t.Name == "TFSAdvanced.Models"))).AsSelf();
            builder.RegisterAssemblyTypes(Assembly.Load(Assembly.GetEntryAssembly().GetReferencedAssemblies().First(t => t.Name == "TFSAdvanced.Updater"))).Where(x => x.Name.EndsWith("Updater")).AsSelf().SingleInstance();
            builder.RegisterAssemblyTypes(Assembly.Load(Assembly.GetEntryAssembly().GetReferencedAssemblies().First(t => t.Name == "TFSAdvanced.Updater"))).Where(x => !x.Name.EndsWith("Updater")).AsSelf();
            builder.RegisterAssemblyTypes(Assembly.Load(Assembly.GetEntryAssembly().GetReferencedAssemblies().First(t => t.Name == "TFSAdvanced.DataStore"))).AsSelf().Where(x => x.Name.EndsWith("Repository")).SingleInstance();
            builder.RegisterAssemblyTypes(Assembly.Load(Assembly.GetEntryAssembly().GetReferencedAssemblies().First(t => t.Name == "TFSAdvanced.DataStore"))).AsSelf().Where(x => !x.Name.EndsWith("Repository"));


            builder.RegisterType<AuthorizationRequest>().AsSelf().SingleInstance();
            builder.RegisterType<BuildDefinitionRequest>().AsSelf().SingleInstance();
            builder.RegisterType<RequestData>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<WebSocketUpdater>().AsSelf().InstancePerLifetimeScope();

            var container = builder.Build();
            var serviceProvider = container.Resolve<IServiceProvider>();

            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter { CamelCaseText = true});
                return settings;
            };

            var dataContext = serviceProvider.GetService<TfsAdvancedSqlDataContext>();
            dataContext.Database.EnsureCreated();
            
            return serviceProvider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                    .Enrich.WithExceptionDetails()
                    .Enrich.With(new MachineNameEnricher())
                    .Enrich.FromLogContext()
                    .MinimumLevel.Is(LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                    .MinimumLevel.Override("System", LogEventLevel.Error)
                    .WriteTo.ColoredConsole()
                    .WriteTo.Trace() // For VS Output
                ;
            Log.Logger = loggerConfiguration.CreateLogger();


            app.UseDeveloperExceptionPage();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSession();
            app.UseAuthenticationMiddleware();
            app.UseMvc();
            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var pullRequestSocket = context.RequestServices.GetService<WebSocketUpdater>();
                        await pullRequestSocket.RegisterSocket(context, webSocket);
                    }
                    else
                    {
                        {
                            context.Response.StatusCode = 400;
                        }
                    }
                }
                else
                {
                    await next();
                }
            });

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new []{new HangfireAuthorizationFilter()}
            });
            app.UseHangfireServer();

   

            var seqKey = Configuration["Logging:Seq:Key"];
            if(!string.IsNullOrEmpty(seqKey))
            {
                var seqUrl = Configuration["Logging:Seq:Url"];
                loggerConfiguration.WriteTo.Seq(serverUrl:seqUrl, apiKey:seqKey);
            }

            var applicationInsights = Configuration["Logging:ApplicationInsights:InstrumentationKey"];
            if (!string.IsNullOrEmpty(applicationInsights))
            {
                loggerConfiguration.WriteTo.ApplicationInsightsTraces(instrumentationKey: applicationInsights);
            }

            if (env.IsDevelopment())
            {
                loggerConfiguration.WriteTo.Trace();
            }

            TelemetryConfiguration.Active.DisableTelemetry = true;


            GlobalJobFilters.Filters.Add(new HangfireJobFilter());

            BackgroundJob.Enqueue<Updater.Tasks.Updater>(updater => updater.Start());
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[1024 * 4];

                DateTime now = DateTime.Now;
                var response = Encoding.UTF8.GetBytes(now.ToString());
                await webSocket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
                Thread.Sleep(1000);
            }
            
        }
    }
}
