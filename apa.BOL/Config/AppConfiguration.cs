using System.Collections.Generic;

namespace apa.BOL.Config
{
    //--------------------------------------------------------------------------------------------------------------
    public class AppConfiguration
    {
        public ConnectionStrings ConnectionStrings { get; set; }
    }

    //--------------------------------------------------------------------------------------------------------------
    public class ConnectionStrings
    {
        public string CnxDb { get; set; }
    }

    //--------------------------------------------------------------------------------------------------------------
    public class MongoDBSettings
    {
        public string CnxString { get; set; }
        public string Database { get; set; }
    }

    //--------------------------------------------------------------------------------------------------------------
    public class CosmosDBSettings
    {
        public CosmosDBSettings()
        {
            string nc = "Not configured";
            ServiceEndpoint = nc;
            AuthKey = nc;
            Database = nc;
            Collection = nc;
        }

        public string ServiceEndpoint { get; set; }
        public string AuthKey { get; set; }
        public string CnxString { get; set; }
        public string Database { get; set; }
        public string Collection { get; set; }
    }

    //--------------------------------------------------------------------------------------------------------------
    public class ApplicationInsights
    {
        public string InstrumentationKey { get; set; }
    }

    //--------------------------------------------------------------------------------------------------------------
    public class AppSettingsConfig
    {
        public Dictionary<string, object> KeyVaultObjects { get; set; }
        public KeyVault KeyVault { get; set; }
    }

    //--------------------------------------------------------------------------------------------------------------
    public class KeyVault
    {
        public string Vault { get; set; }
        public string AppId { get; set; }
        public string AppSecret { get; set; }
    }

    //--------------------------------------------------------------------------------------------------------------
    public class PowerBI
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string GroupId { get; set; }
        public string ReportID { get; set; }
        public string PowerBiAPI { get; set; }
        public string PowerBiDataset { get; set; }
        public string PbiUsername { get; set; }
        public string PbiPassword { get; set; }
        public string AADAuthorityUri { get; set; }
        public string ResourceUrl { get; set; }
        public string ApiUrl { get; set; }
        public string RedirectUrl { get; set; }
    }
}
