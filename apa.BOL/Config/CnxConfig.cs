namespace apa.BOL.Config
{
    //----------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Stores the connection data for the current application session.
    /// </summary>
    public class CnxConfig
    {
        public string CnxDb { get; set; }
        public bool HasWorkstationID { get; set; }
        public int Timeout { get; set; }

        //------------------------------------------------------------------------------------------------------
        public CnxConfig()
        {
            Timeout = -1;
            HasWorkstationID = false;
        }
    }
}
