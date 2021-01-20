﻿using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace azurestream.console
{
    //--------------------------------------------------------------------------------------------------------------
    public class AIKeyPhraseExtractionSample
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

            var inputDocuments = new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>
                        {
                            new MultiLanguageInput("1", "猫は幸せ", "ja"),
                            new MultiLanguageInput("2", "Fahrt nach Stuttgart und dann zum Hotel zu Fu.", "de"),
                            new MultiLanguageInput("3", "My cat might need to see a veterinarian.", "en"),
                            new MultiLanguageInput("4", "A mi me encanta el fútbol!", "es")
                        });

            var kpResults = await client.KeyPhrasesBatchAsync(inputDocuments);

            // Printing keyphrases
            Console.WriteLine("===== Key Phrases Extraction =====\n");

            foreach (var document in kpResults.Documents)
            {
                Console.WriteLine($"Document ID: {document.Id} ");

                Console.WriteLine("\t Key phrases:");

                foreach (string keyphrase in document.KeyPhrases)
                {
                    Console.WriteLine($"\t\t{keyphrase}");
                }
            }
            Console.WriteLine();
        }
    }
}