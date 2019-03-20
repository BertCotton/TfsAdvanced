using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aggregator.Messages;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Configuration;
using Akka.DI.AutoFac;
using Akka.DI.Core;
using Akka.Routing;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Serilog;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Exceptions;
using TFSAdvancedAggregator.Actors;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;

namespace Aggregator
{
    public class Startup
    {
        private readonly IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var config = ConfigurationFactory.ParseString(@"
akka {
    actor.debug.unhandled = on
    stdout-loglevel = DEBUG
    loggers=[""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]
    loglevel = DEBUG
    log-config-on-start = on  
    actor.provider = cluster
    actor {                
        debug {  
              receive = on 
              autoreceive = on
              lifecycle = on
              event-stream = on
              unhandled = on
              log-received-messages = on
        }
    }  
    remote {
        dot-netty.tcp {
            port = 8081
            hostname = localhost
        }
    }
    cluster {
        seed-nodes = [""akka.tcp://TFSAdvanced@localhost:8081""]
        pub-sub {
            send-to-dead-letters-when-no-subscribers = on
        }
    }
}");
            var system = ActorSystem.Create("TFSAdvanced", config);
            
            services.AddSingleton(system);
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddScoped<RequestData>();
            services.AddScoped<RepositoryUpdaterActor>();
            services.AddScoped<RepositoryUpdaterWorkerActor>();
            services.AddScoped<ProjectUpdaterActor>();
            services.AddScoped<ProjectUpdaterWorkerActor>();
            services.AddScoped<BuildDefinitionUpdaterActor>();
            services.AddScoped<BuildDefinitionUpdaterWorkerActor>();
            services.AddScoped<BuildUpdaterActor>();
            services.AddScoped<BuildUpdaterWorkerActor>();
            services.AddScoped<PullRequestUpdaterActor>();
            services.AddScoped<PullRequestUpdaterWorkerActor>();
            var builder = new ContainerBuilder();
            builder.Populate(services);

            var container = builder.Build();
           
            system.AddDependencyResolver(new AutoFacDependencyResolver(container, system));
            
            
            var serviceProvider = container.Resolve<IServiceProvider>();

            return serviceProvider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
         
            lifetime.ApplicationStarted.Register(() =>
            {
                var actorSystem = app.ApplicationServices.GetService<ActorSystem>(); // start Akka.NET
                
                // Register Actors
                RegisterActor<BuildDefinitionUpdaterActor>(actorSystem);
                RegisterActor<BuildUpdaterActor>(actorSystem);
                RegisterActor<RepositoryUpdaterActor>(actorSystem);
                RegisterActor<PullRequestUpdaterActor>(actorSystem);
                
                
                // Start the process
                var projectUpdaterProps = actorSystem.DI().Props<ProjectUpdaterActor>();
                var projectUpdater = actorSystem.ActorOf(projectUpdaterProps, "ProjectUpdater");
                projectUpdater.Tell(new SimpleMessages.SUBSCRIBE(), ActorRefs.NoSender);
                actorSystem.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(3), TimeSpan.FromMinutes(30), projectUpdater, new SimpleMessages.UPDATE_PROJECTS(), ActorRefs.NoSender);
               

            });
            lifetime.ApplicationStopping.Register(() =>
            {
                app.ApplicationServices.GetService<ActorSystem>().Terminate().Wait();
            });
            
        }

        private void RegisterActor<T>(ActorSystem system) where T: ReceiveActor
        {
            var prop = system.DI().Props<T>();
            var actor = system.ActorOf(prop, typeof(T).Name);
//            actor.Tell(new Subscribe("TFSAdvanced", ActorRefs.NoSender));
            actor.Tell(new SimpleMessages.SUBSCRIBE(), ActorRefs.NoSender);
        }
    }
}