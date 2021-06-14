using System;

namespace azurestream.eventhubs.Helpers
{
    //--------------------------------------------------------------------------------------------------------------
    public static class Randomizer
    {
        //----------------------------------------------------------------------------------------------------------
        // Generate a random number between two numbers
        public static int Number(int min, int max)
        {
            return new Random().Next(min, max + 1);
        }

        //----------------------------------------------------------------------------------------------------------
        // Generate a random guid with no hyphens
        public static string Guid()
        {
            return System.Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }
}