using apa.BOL.Config;
using apa.DAL;
using apa.DAL.Repository;
using azurestream.Services;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace azurestream
{
    //--------------------------------------------------------------------------------------------------------------
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }
        public int DurationInSeconds { get; set; } = 60 * 60 * 24;

        //----------------------------------------------------------------------------------------------------------
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Environment = env;
            Configuration = configuration;

            //example of how to read values from Azure Key Vault
            var cnxDb = Configuration["ConnectionStrings:CnxDb"];
            var redisCnx = Configuration["ConnectionStrings:RedisCnx"];
            var sendgriduser = Configuration["SendGrid:SendGridUser"];
            var sendgridpassword = Configuration["SendGrid:SendGridPassword"];
            var sendgridkey = Configuration["SendGrid:SendGridKey"];
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        //----------------------------------------------------------------------------------------------------------
        public void ConfigureServices(IServiceCollection services)
        {
            //Add framework services.
            //services.AddDbContext<ApplicationDbContext>(options =>
            //   options.UseSqlServer(Configuration.GetConnectionString("CnxDb")));

            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = Configuration.GetConnectionString("RedisCnx");
                option.InstanceName = "master";
            });

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"));

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();

            services.AddRazorPages()
                .AddMicrosoftIdentityUI();

            services.AddServerSideBlazor();
            services.AddSignalR().AddAzureSignalR();
			
            //The following line enables Application Insights telemetry collection.
            services.AddApplicationInsightsTelemetry(Configuration["ApplicationInsights:Cnx"]);

            if (Environment.IsDevelopment())
            {
                // Development configuration
            }
            else
            {
                // Staging/Production configuration
            }

            //Add settings from configuration
            services.Configure<AppConfiguration>(Configuration.GetSection("AppSettings"));
            services.Configure<CnxConfig>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<EmailConfig>(Configuration.GetSection("Email"));
            services.Configure<SendGridConfig>(Configuration.GetSection("SendGrid"));
            services.Configure<CloudStorageConfig>(Configuration.GetSection("CloudStorage"));
            services.Configure<ThresholdOptions>(Configuration.GetSection("AlertThresholds"));
            services.Configure<ApplicationInsights>(Configuration.GetSection("ApplicationInsights"));

            //Add application services.
			services.AddSingleton<ICosmosDbService>(InitializeCosmosClientInstanceAsync(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<ISmsSender, EmailSender>();
            services.AddScoped<IViewRender, XViewRenderer>();
			services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IViewModelService, ViewModelService>();

            //Add Application Repositories.
            services.AddSingleton<ISensorRepository, SensorDataRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //----------------------------------------------------------------------------------------------------------
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.Use((ctx, next) =>
            {
                var requestTelemetry = ctx.Features.Get<RequestTelemetry>();
                if (requestTelemetry != null && DocumentClientEventListener.Request.Value == null)
                {
                    DocumentClientEventListener.Request.Value = requestTelemetry;
                }

                return next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); //Enable the Developer Exception Page only when the app is running in the Development environment. You don't want to share detailed exception information publicly when the app runs in production.
            }
            else
            {
                app.UseStatusCodePagesWithRedirects("~/home/error/{0}");
                //app.UseExceptionHandler("/Home/Error"); //you can use this or use above
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

           

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Areas support
                endpoints.MapControllerRoute(
                  name: "areaRoute",
                  pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });
        }

        // <InitializeCosmosClientInstanceAsync>
        /// <summary>
        /// Creates a Cosmos DB database and a container with the specified partition key.
        /// </summary>
        //----------------------------------------------------------------------------------------------------------
        private static async Task<XCosmosDbService> InitializeCosmosClientInstanceAsync(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;
            string containerName = configurationSection.GetSection("ContainerName").Value;
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;

            CosmosClientBuilder clientBuilder = new CosmosClientBuilder(account, key);
            CosmosClientOptions options = new CosmosClientOptions() 
                {
                    ApplicationName = "ToDoList",
                    AllowBulkExecution = false,
                    ApplicationRegion = Regions.EastUS,
                    //ApplicationPreferredRegions = new List<string> { Regions.WestUS, Regions.WestUS2 }
            };

            CosmosClient client = clientBuilder
                                //.WithConnectionModeGateway()
                                .WithConnectionModeDirect() //Direct is the default - Direct mode is the preferred option for best performance.
                                .WithApplicationName(options.ApplicationName)
                                //.WithApplicationPreferredRegions(options.ApplicationPreferredRegions)
                                .WithApplicationRegion(options.ApplicationRegion)
                                .WithBulkExecution(options.AllowBulkExecution)
                                .Build();

            var cosmosDbService = new XCosmosDbService(client, databaseName, containerName);
            var database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

            return cosmosDbService;
        }
    }
}
