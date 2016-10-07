using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TfsAdvanced.Data;
using TfsAdvanced.Infrastructure;

namespace TfsAdvanced
{
    public class Startup
    {
        private string siteName = Environment.GetEnvironmentVariable("SiteName") ?? "ius";
        public IConfigurationRoot Configuration { get; set; }

        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{siteName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            builder.AddApplicationInsightsSettings(developerMode: true);

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            });
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddMemoryCache();

            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            var builder = new ContainerBuilder();

            builder.Populate(services);

            builder.RegisterType<CacheStats>().AsSelf().SingleInstance();
            builder.RegisterType<Cache>().AsSelf().SingleInstance();

            builder.RegisterType<SignInManager<User>>().AsSelf();

            builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
                .Where(t => t.Name.EndsWith("Request") || t.Name.EndsWith("Repository"))
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<RequestData>().AsSelf().InstancePerLifetimeScope();

            var container = builder.Build();
            var serviceProvider = container.Resolve<IServiceProvider>();
            return serviceProvider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDeveloperExceptionPage();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
                app.UseClientCertMiddleware();

            app.UseApplicationInsightsExceptionTelemetry();
            app.UseApplicationInsightsRequestTelemetry();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Cookies",
                LoginPath = new PathString("/data/Login"),
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                CookieName = "TfsAdvance.Cookies"
            });
            

            app.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions
            {
                AuthenticationScheme = "Microsoft",
                SignInScheme = "Cookies",
                ClientId = "",
                ClientSecret = ""
            });

            app.UseMvc();
        }
    }
}