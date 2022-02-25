namespace azurestream.console
{
    //--------------------------------------------------------------------------------------------------------------
    public class DeviceProvisioningSample1
    {
        //----------------------------------------------------------------------------------------------------------
        internal static async Task ExecuteAsync(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.WriteLine("You must specify an action!");
                return;
            }

            if (args[0] == "setup")
            {
                CertificateFactory.CreateTestCert();
            }
            else
            {
                var scopeId = args[0];
                var device = new Device(scopeId);
                await device.Provision();
                await device.ConnectAndRun();
            }
        }
    }
}
