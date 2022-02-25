using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;

namespace azurestream.funcapp
{
    //--------------------------------------------------------------------------------------------------------------
    public class BlobTrigger
    {
        //----------------------------------------------------------------------------------------------------------
        [FunctionName("BlobTrigger")]
        public void Run([BlobTrigger("images/{name}", Connection = "BlobCnx")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
