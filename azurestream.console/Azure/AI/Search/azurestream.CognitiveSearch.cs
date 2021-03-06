﻿using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Spatial;
using System;
using System.Threading;

namespace azurestream.console
{
    //----------------------------------------------------------------------------------------------------------
    //https://github.com/Azure-Samples/search-dotnet-getting-started
    public class CognitiveSearch
    {
        //------------------------------------------------------------------------------------------------------
        internal static void Execute()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            SearchIndexClient indexClient = CreateSearchIndexClient(configuration);

            string indexName = configuration["SearchIndexName"];

            Console.WriteLine("{0}", "Deleting index...\n");
            DeleteIndexIfExists(indexName, indexClient);

            Console.WriteLine("{0}", "Creating index...\n");
            CreateIndex(indexName, indexClient);

            SearchClient searchClient = indexClient.GetSearchClient(indexName);

            Console.WriteLine("{0}", "Uploading documents...\n");
            UploadDocuments(searchClient);

            SearchClient indexClientForQueries = CreateSearchClientForQueries(indexName, configuration);

            //RunQueries(indexClientForQueries);

            Console.WriteLine("{0}", "Complete.  Press any key to end application...\n");
            Console.ReadKey();
        }

        //------------------------------------------------------------------------------------------------------
        private static SearchIndexClient CreateSearchIndexClient(IConfigurationRoot configuration)
        {
            string searchServiceEndPoint = configuration["SearchServiceEndPoint"];
            string adminApiKey = configuration["SearchServiceAdminApiKey"];

            SearchIndexClient indexClient = new SearchIndexClient(new Uri(searchServiceEndPoint), new AzureKeyCredential(adminApiKey));
            return indexClient;
        }

        private static SearchClient CreateSearchClientForQueries(string indexName, IConfigurationRoot configuration)
        {
            string searchServiceEndPoint = configuration["SearchServiceEndPoint"];
            string queryApiKey = configuration["SearchServiceQueryApiKey"];

            SearchClient searchClient = new SearchClient(new Uri(searchServiceEndPoint), indexName, new AzureKeyCredential(queryApiKey));
            return searchClient;
        }

        //------------------------------------------------------------------------------------------------------
        private static void DeleteIndexIfExists(string indexName, SearchIndexClient indexClient)
        {
            try
            {
                if (indexClient.GetIndex(indexName) != null)
                {
                    indexClient.DeleteIndex(indexName);
                }
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                //if exception occurred and status is "Not Found", this is work as expect
                Console.WriteLine("Failed to find index and this is because it's not there.");
            }
        }

        private static void CreateIndex(string indexName, SearchIndexClient indexClient)
        {
            FieldBuilder fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(Models.Hotel));

            var definition = new SearchIndex(indexName, searchFields);

            indexClient.CreateOrUpdateIndex(definition);
        }

        //------------------------------------------------------------------------------------------------------
        private static void UploadDocuments(SearchClient searchClient)
        {
            IndexDocumentsBatch<Models.Hotel> batch = IndexDocumentsBatch.Create(
                IndexDocumentsAction.Upload(
                    new Models.Hotel()
                    {
                        HotelId = "1",
                        HotelName = "Secret Point Motel",
                        Description = "The hotel is ideally located on the main commercial artery of the city in the heart of New York. A few minutes away is Time's Square and the historic centre of the city, as well as other places of interest that make New York one of America's most attractive and cosmopolitan cities.",
                        DescriptionFr = "L'hôtel est idéalement situé sur la principale artère commerciale de la ville en plein cœur de New York. A quelques minutes se trouve la place du temps et le centre historique de la ville, ainsi que d'autres lieux d'intérêt qui font de New York l'une des villes les plus attractives et cosmopolites de l'Amérique.",
                        Category = "Boutique",
                        Tags = new[] { "pool", "air conditioning", "concierge" },
                        ParkingIncluded = false,
                        LastRenovationDate = new DateTimeOffset(1970, 1, 18, 0, 0, 0, TimeSpan.Zero),
                        Rating = 3.6,
                        Location = GeographyPoint.Create(40.760586, -73.975403),
                        Address = new Models.Address()
                        {
                            StreetAddress = "677 5th Ave",
                            City = "New York",
                            StateProvince = "NY",
                            PostalCode = "10022",
                            Country = "USA"
                        },
                        Rooms = new Models.Room[]
                        {
                            new Models.Room()
                            {
                                Description = "Budget Room, 1 Queen Bed (Cityside)",
                                DescriptionFr = "Chambre Économique, 1 grand lit (côté ville)",
                                Type = "Budget Room",
                                BaseRate = 96.99,
                                BedOptions = "1 Queen Bed",
                                SleepsCount = 2,
                                SmokingAllowed = true,
                                Tags = new[] { "vcr/dvd" }
                            },
                            new Models.Room()
                            {
                                Description = "Budget Room, 1 King Bed (Mountain View)",
                                DescriptionFr = "Chambre Économique, 1 très grand lit (Mountain View)",
                                Type = "Budget Room",
                                BaseRate = 80.99,
                                BedOptions = "1 King Bed",
                                SleepsCount = 2,
                                SmokingAllowed = true,
                                Tags = new[] { "vcr/dvd", "jacuzzi tub" }
                            },
                            new Models.Room()
                            {
                                Description = "Deluxe Room, 2 Double Beds (City View)",
                                DescriptionFr = "Chambre Deluxe, 2 lits doubles (vue ville)",
                                Type = "Deluxe Room",
                                BaseRate = 150.99,
                                BedOptions = "2 Double Beds",
                                SleepsCount = 2,
                                SmokingAllowed = false,
                                Tags = new[] { "suite", "bathroom shower", "coffee maker" }
                            }
                        }
                    }),
                IndexDocumentsAction.Upload(
                    new Models.Hotel()
                    {
                        HotelId = "2",
                        HotelName = "Twin Dome Motel",
                        Description = "The hotel is situated in a  nineteenth century plaza, which has been expanded and renovated to the highest architectural standards to create a modern, functional and first-class hotel in which art and unique historical elements coexist with the most modern comforts.",
                        DescriptionFr = "L'hôtel est situé dans une place du XIXe siècle, qui a été agrandie et rénovée aux plus hautes normes architecturales pour créer un hôtel moderne, fonctionnel et de première classe dans lequel l'art et les éléments historiques uniques coexistent avec le confort le plus moderne.",
                        Category = "Boutique",
                        Tags = new[] { "pool", "free wifi", "concierge" },
                        ParkingIncluded = false,
                        LastRenovationDate = new DateTimeOffset(1979, 2, 18, 0, 0, 0, TimeSpan.Zero),
                        Rating = 3.60,
                        Location = GeographyPoint.Create(27.384417, -82.452843),
                        Address = new Models.Address()
                        {
                            StreetAddress = "140 University Town Center Dr",
                            City = "Sarasota",
                            StateProvince = "FL",
                            PostalCode = "34243",
                            Country = "USA"
                        },
                        Rooms = new Models.Room[]
                        {
                            new Models.Room()
                            {
                                Description = "Suite, 2 Double Beds (Mountain View)",
                                DescriptionFr = "Suite, 2 lits doubles (vue sur la montagne)",
                                Type = "Suite",
                                BaseRate = 250.99,
                                BedOptions = "2 Double Beds",
                                SleepsCount = 2,
                                SmokingAllowed = false,
                                Tags = new[] { "Room Tags" }
                            },
                            new Models.Room()
                            {
                                Description = "Standard Room, 1 Queen Bed (City View)",
                                DescriptionFr = "Chambre Standard, 1 grand lit (vue ville)",
                                Type = "Standard Room",
                                BaseRate = 121.99,
                                BedOptions = "1 Queen Bed",
                                SleepsCount = 2,
                                SmokingAllowed = false,
                                Tags = new[] { "jacuzzi tub" }
                            },
                            new Models.Room()
                            {
                                Description = "Budget Room, 1 King Bed (Waterfront View)",
                                DescriptionFr = "Chambre Économique, 1 très grand lit (vue sur le front de mer)",
                                Type = "Budget Room",
                                BaseRate = 88.99,
                                BedOptions = "1 King Bed",
                                SleepsCount = 2,
                                SmokingAllowed = false,
                                Tags = new[] { "suite", "tv", "jacuzzi tub" }
                            }
                        }
                    }),
                IndexDocumentsAction.Upload(
                    new Models.Hotel()
                    {
                        HotelId = "3",
                        HotelName = "Triple Landscape Hotel",
                        Description = "The Hotel stands out for its gastronomic excellence under the management of William Dough, who advises on and oversees all of the Hotel’s restaurant services.",
                        DescriptionFr = "L'hôtel est situé dans une place du XIXe siècle, qui a été agrandie et rénovée aux plus hautes normes architecturales pour créer un hôtel moderne, fonctionnel et de première classe dans lequel l'art et les éléments historiques uniques coexistent avec le confort le plus moderne.",
                        Category = "Resort and Spa",
                        Tags = new[] { "air conditioning", "bar", "continental breakfast" },
                        ParkingIncluded = true,
                        LastRenovationDate = new DateTimeOffset(2015, 9, 20, 0, 0, 0, TimeSpan.Zero),
                        Rating = 4.80,
                        Location = GeographyPoint.Create(33.84643, -84.362465),
                        Address = new Models.Address()
                        {
                            StreetAddress = "3393 Peachtree Rd",
                            City = "Atlanta",
                            StateProvince = "GA",
                            PostalCode = "30326",
                            Country = "USA"
                        },
                        Rooms = new Models.Room[]
                        {
                            new Models.Room()
                            {
                                Description = "Standard Room, 2 Queen Beds (Amenities)",
                                DescriptionFr = "Chambre Standard, 2 grands lits (Services)",
                                Type = "Standard Room",
                                BaseRate = 101.99,
                                BedOptions = "2 Queen Beds",
                                SleepsCount = 4,
                                SmokingAllowed = true,
                                Tags = new[] { "vcr/dvd", "vcr/dvd" }
                            },
                            new Models.Room ()
                            {
                                Description = "Standard Room, 2 Double Beds (Waterfront View)",
                                DescriptionFr = "Chambre Standard, 2 lits doubles (vue sur le front de mer)",
                                Type = "Standard Room",
                                BaseRate = 106.99,
                                BedOptions = "2 Double Beds",
                                SleepsCount = 2,
                                SmokingAllowed = true,
                                Tags = new[] { "coffee maker" }
                            },
                            new Models.Room()
                            {
                                Description = "Deluxe Room, 2 Double Beds (Cityside)",
                                DescriptionFr = "Chambre Deluxe, 2 lits doubles (Cityside)",
                                Type = "Budget Room",
                                BaseRate = 180.99,
                                BedOptions = "2 Double Beds",
                                SleepsCount = 2,
                                SmokingAllowed = true,
                                Tags = new[] { "suite" }
                             }
                        }
                    }));

            try
            {
                IndexDocumentsResult result = searchClient.IndexDocuments(batch);
            }
            catch (Exception)
            {
                // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                // the batch. Depending on your application, you can take compensating actions like delaying and
                // retrying. For this simple demo, we just log the failed document keys and continue.
                Console.WriteLine("Failed to index some of the documents: {0}");
            }

            Console.WriteLine("Waiting for documents to be indexed...\n");
            Thread.Sleep(2000);
        }
    }
}
