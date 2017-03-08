using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TfsAdvanced.Data;
using TfsAdvanced.Infrastructure;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;

namespace TfsAdvanced
{
    public class Startup
    {
        public static readonly int MAX_DEGREE_OF_PARALLELISM = -1;
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

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<TfsAdvancedDataContext>();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<TfsAdvancedDataContext>()
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

            builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
                .Where(t => t.Name.EndsWith("Request") || t.Name.EndsWith("Repository") || t.Name.EndsWith("Updater"))
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
            app.UseSession();
            app.UseAuthenticationMiddleware();
            app.UseMvc();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new []{new HangfireAuthorizationFilter()}
            });
            app.UseHangfireServer();

            Hangfire.BackgroundJob.Enqueue<Updater.Tasks.Updater>(updater => updater.Start());
        }
    }
}
