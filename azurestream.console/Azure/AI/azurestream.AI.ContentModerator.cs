using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace azurestream.console
{
    //--------------------------------------------------------------------------------------------------------------
    public class AIContentModerator
    {
        //https://docs.microsoft.com/en-us/azure/cognitive-services/Content-Moderator/client-libraries?pivots=programming-language-csharp#authenticate-the-client
        //Content Moderator is a cognitive service that checks text, image, and video content for material that is potentially offensive, risky, or otherwise undesirable. 
        //When such material is found, the service applies appropriate labels (flags) to the content. Your app can then handle flagged content to comply with regulations or maintain the intended environment for users.
        // TEXT MODERATION
        // Name of the file that contains text
        private static readonly string TextFile = "AITextFile.txt";
        // The name of the file to contain the output from the evaluation.
        private static string TextOutputFile = "AITextModerationOutput.txt";

        // Your Content Moderator subscription key is found in your Azure portal resource on the 'Keys' page. Add to your environment variables.
        private static readonly string SubscriptionKey = Environment.GetEnvironmentVariable("YOURCOGNATIVESERVICESKEY");
        // Base endpoint URL. Add this to your environment variables. Found on 'Overview' page in Azure resource. For example: https://westus.api.cognitive.microsoft.com
        private static readonly string Endpoint = Environment.GetEnvironmentVariable("https://eastus.api.cognitive.microsoft.com/");
        //----------------------------------------------------------------------------------------------------------
        internal static async Task Execute()
        {
            // Create an image review client
            ContentModeratorClient clientImage = Authenticate(SubscriptionKey, Endpoint);
            // Create a text review client
            ContentModeratorClient clientText = Authenticate(SubscriptionKey, Endpoint);
            // Create a human reviews client
            ContentModeratorClient clientReviews = Authenticate(SubscriptionKey, Endpoint);

            // Moderate text from text in a file
            await ModerateText(clientText, TextFile, TextOutputFile);
        }


        /*
         * TEXT MODERATION
         * This example moderates text from file.
        */
        public static async Task ModerateText(ContentModeratorClient client, string inputFile, string outputFile)
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("TEXT MODERATION");
            Console.WriteLine();
            // Load the input text.
            string text = File.ReadAllText(inputFile);

            // Remove carriage returns
            text = text.Replace(Environment.NewLine, " ");
            // Convert string to a byte[], then into a stream (for parameter in ScreenText()).
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            MemoryStream stream = new MemoryStream(textBytes);

            Console.WriteLine("Screening {0}...", inputFile);
            // Format text

            // Save the moderation results to a file.
            using (StreamWriter outputWriter = new StreamWriter(outputFile, false))
            {
                using (client)
                {
                    // Screen the input text: check for profanity, classify the text into three categories,
                    // do autocorrect text, and check for personally identifying information (PII)
                    outputWriter.WriteLine("Autocorrect typos, check for matching terms, PII, and classify.");

                    // Moderate the text
                    var screenResult = client.TextModeration.ScreenText("text/plain", stream, "eng", true, true, null, true);
                    outputWriter.WriteLine(JsonConvert.SerializeObject(screenResult, Formatting.Indented));
                }

                await outputWriter.FlushAsync();
                outputWriter.Close();
            }

            Console.WriteLine("Results written to {0}", outputFile);
            Console.WriteLine();
        }

        public static ContentModeratorClient Authenticate(string key, string endpoint)
        {
            ContentModeratorClient client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(key));
            client.Endpoint = endpoint;
            return client;
        }
    }

    // Contains the image moderation results for an image, 
    // including text and face detection results.
    public class EvaluationData
    {
        // The URL of the evaluated image.
        public string ImageUrl;

        // The image moderation results.
        public Evaluate ImageModeration;

        // The text detection results.
        public OCR TextDetection;

        // The face detection results;
        public FoundFaces FaceDetection;
    }

    // Associates the review ID (assigned by the service) to the internal.
    public class ReviewItem
    {
        // The media type for the item to review. 
        public string Type;
        // The URL of the item to review.
        public string Url;
        // The internal content ID for the item to review.
        public string ContentId;
        // The ID that the service assigned to the review.
        public string ReviewId;
    }
}
