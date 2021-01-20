using azurestream.console.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Index = Microsoft.Azure.Search.Models.Index;

namespace azurestream.console
{
    //----------------------------------------------------------------------------------------------------------
    public class SearchIndex
    {
        private static readonly string hotelFileName = "C:\\Git\\azurestream\\azurestream.console\\SampleData\\HotelData.txt";
        private static readonly string searchService = "YOURSEARCHSERVICE";
        private static readonly string searchKey = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
        //------------------------------------------------------------------------------------------------------
        internal static void Execute()
        {
            var serviceClient = CreateSearchServiceClient();
            //1. Create Index first and then comment out
            CreateIndex(serviceClient);
            //2. Import data but make sure to comment CreateIndex line above
            ImportIndexData(serviceClient);
            Console.Read();
        }

        //------------------------------------------------------------------------------------------------------
        private static SearchServiceClient CreateSearchServiceClient()
        {
            SearchServiceClient serviceClient = new SearchServiceClient(searchService, new SearchCredentials(searchKey));
            return serviceClient;
        }

        //------------------------------------------------------------------------------------------------------
        private static void CreateIndex(SearchServiceClient serviceClient)
        {
            var definition = new Index()
            {
                Name = "hotels-index",
                Fields = FieldBuilder.BuildForType<Hotel>()
            };

            serviceClient.Indexes.Create(definition);
        }

        //------------------------------------------------------------------------------------------------------
        private static void ImportIndexData(SearchServiceClient serviceClient)
        {
            var hotelsText = File.ReadAllLines(hotelFileName);
            var hotels = new List<Hotel>();
            for (int i = 1; i < hotelsText.Length; i++)
            {
                var hotelText = hotelsText[i];
                var hotelTextColumns = hotelText.Split("\t");
                hotels.Add(
                new Hotel()
                {
                    HotelId = hotelTextColumns[0],
                    HotelName = hotelTextColumns[1],
                    Description = hotelTextColumns[2],
                    DescriptionFr = hotelTextColumns[3],
                    Category = hotelTextColumns[4],
                    Tags = hotelTextColumns[5].Split(","),
                    ParkingIncluded = hotelTextColumns[6] == "0" ? false : true,
                    SmokingAllowed = hotelTextColumns[7] == "0" ? false : true,
                    LastRenovationDate = Convert.ToDateTime(hotelTextColumns[8]),
                    BaseRate = Convert.ToDouble(hotelTextColumns[9]),
                    Rating = (int)Convert.ToDouble(hotelTextColumns[10])
                });
            }

            var actions = new List<IndexAction<Hotel>>();
            foreach (var hotel in hotels)
            {
                actions.Add(IndexAction.Upload(hotel));
            }

            var batch = IndexBatch.New(actions);

            try
            {
                ISearchIndexClient indexClient = serviceClient.Indexes.GetClient("hotels-index");
                indexClient.Documents.Index(batch);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
