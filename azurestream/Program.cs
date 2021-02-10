using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using azurestream.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.AzureAppServices;
using NLog.Extensions.Logging;
using NLog.Web;
using System;
using System.IO;

namespace azurestream
{
    //--------------------------------------------------------------------------------------------------------------
    public class Program
    {
        private static DocumentClientEventListener DocumentClientEventListener;
        //----------------------------------------------------------------------------------------------------------
        public static void Main(string[] args)
        {
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                DocumentClientEventListener = new DocumentClientEventListener();
                logger.Debug("init main");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        private static string GetKeyVaultEndpoint() => "https://azurestreamkeyvault.vault.azure.net/";
        //----------------------------------------------------------------------------------------------------------
        public static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
             .ConfigureAppConfiguration((context, config) =>
             {
                 var env = context.HostingEnvironment;

                 config.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                   .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                 var builtConfig = config.Build();

                 //IF IM DEVELOPING LOCALLY GO IN HERE AND GRAP CREDENTIALS FOR KEY VAULT AND BINGO! IM IN KEY VAULT
                 if (env.IsDevelopment())
                 {
                     //https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-5.0
                     //https://microsoft.github.io/AzureTipsAndTricks/blog/tip181.html
                     //https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/identity/Azure.Identity#environment-variables
                     //EnvironmentCrendentials are set from Environment Variables inside of Visual Studio. Right click on project (azurestream) and go to Debug Tab and then Environment Variables to set values.
                     //var keyVaultClient = new SecretClient(new Uri($"https://{builtConfig["azureKeyVault:vault"]}.vault.azure.net/"), new DefaultAzureCredential());
                     var keyVaultClient = new SecretClient(new Uri($"https://{builtConfig["azureKeyVault:vault"]}.vault.azure.net/"), new EnvironmentCredential());
                     config.AddAzureKeyVault(keyVaultClient, new KeyVaultSecretManager());
                 }

                 //IF I'VE DEPLOYED TO AZURE GO IN HERE IF I'M USING A MANAGED IDENTITY AND BINGO! IM IN KEY VAULT
                 if (env.IsProduction())
                 {
                     //Getting Key Vault using Managed Identity
                     var keyVaultEndpoint = GetKeyVaultEndpoint();
                     if (!string.IsNullOrEmpty(keyVaultEndpoint))
                     {
                         var keyVaultClient = new SecretClient(new Uri(keyVaultEndpoint), new DefaultAzureCredential());
                         config.AddAzureKeyVault(keyVaultClient, new KeyVaultSecretManager());
                     }
                 }
             })
             .ConfigureLogging(logging =>
             {
                 logging.ClearProviders();
                 logging.AddConsole();
                 logging.AddDebug();
                 logging.AddAzureWebAppDiagnostics(); //The provider only works when the project runs in the Azure environment. It has no effect when the project is run locally—it doesn't write to local files or local development storage for blobs.  
                 //logging.AddNLog();
             })
             .UseNLog()
             .ConfigureServices(serviceCollection => serviceCollection
             .Configure<AzureBlobLoggerOptions>(options =>
                {
                    options.BlobName = "apalog.txt";
                })
             )
             .ConfigureWebHostDefaults(webBuilder =>
             {
                 webBuilder.UseStartup<Startup>();
             });
    }
}
