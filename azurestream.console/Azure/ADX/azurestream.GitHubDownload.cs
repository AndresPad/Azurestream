using System.Net;

namespace azurestream.console.Azure
{
    public class AzADX_GitHubToADX
    {
        internal static void Execute()
        {
            //string remoteUri = "http://data.gharchive.org/2020-01-07-12.json.gz";
            string remoteUriA = "http://data.gharchive.org/";
            string remoteUirB = "-12.json.gz";
            string fileName = null;
            //string myStringWebResource = null;

            DateTime StartDate = Convert.ToDateTime("09-13-2020");
            DateTime EndDate = Convert.ToDateTime("09-16-2020");

            // Create a new WebClient instance.
            using (WebClient myWebClient = new WebClient())
            {
                foreach (DateTime day in EachCalendarDay(StartDate, EndDate))
                {
                    Console.WriteLine(remoteUriA + day.ToString("yyyy-MM-dd") + remoteUirB);

                    fileName = day.ToString("yyyy-MM-dd") + remoteUirB;

                    //Download the Web resource and save it into the current filesystem folder.
                    myWebClient.DownloadFile(remoteUriA + day.ToString("yyyy-MM-dd") + remoteUirB, fileName);
                }
            }
        }

        public static IEnumerable<DateTime> EachCalendarDay(DateTime startDate, DateTime endDate)
        {
            for (var date = startDate.Date; date.Date <= endDate.Date; date = date.AddDays(1)) yield
            return date;
        }
    }
}
