using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace azurestream.console
{
    //--------------------------------------------------------------------------------------------------------------
    public class AIRecognizeEntitiesSample
    {
        private const string Endpoint = "https://eastus.api.cognitive.microsoft.com/";
        private const string SubscriptionKey = "YOURCOGNATIVESERVICESKEY";
        //----------------------------------------------------------------------------------------------------------
        internal static async Task Execute()
        {
            await RunAsync(Endpoint, SubscriptionKey);
        }

        //----------------------------------------------------------------------------------------------------------
        public static async Task RunAsync(string endpoint, string key)
        {
            var credentials = new ApiKeyServiceClientCredentials(key);
            var client = new TextAnalyticsClient(credentials)
            {
                Endpoint = endpoint
            };

            // The documents to be submitted for entity recognition. The ID can be any value.
            var inputDocuments = new MultiLanguageBatchInput(
                new List<MultiLanguageInput>
                {
                    new MultiLanguageInput("1", "Microsoft was founded by Bill Gates and Paul Allen on April 4, 1975, to develop and sell BASIC interpreters for the Altair 8800.", "en"),
                    new MultiLanguageInput("2", "La sede principal de Microsoft se encuentra en la ciudad de Redmond, a 21 kilómetros de Seattle.", "es")
                });

            var entitiesResult = await client.EntitiesBatchAsync(inputDocuments);

            // Printing recognized entities
            Console.WriteLine("===== Named Entity Recognition & Entity Linking =====\n");

            foreach (var document in entitiesResult.Documents)
            {
                Console.WriteLine($"Document ID: {document.Id} ");

                Console.WriteLine("\t Entities:");

                foreach (var entity in document.Entities)
                {
                    Console.WriteLine($"\t\tName: {entity.Name},\tType: {entity.Type ?? "N/A"},\tSub-Type: {entity.SubType ?? "N/A"}");
                    foreach (var match in entity.Matches)
                    {
                        Console.WriteLine($"\t\t\tOffset: {match.Offset},\tLength: {match.Length},\tScore: {match.EntityTypeScore:F3}");
                    }
                }
            }
            Console.WriteLine();
        }
    }
}