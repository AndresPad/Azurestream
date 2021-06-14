// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using apa.BOL.EventHubs;
using System;
using System.Collections.Generic;

namespace azurestream.eventhubs
{
    //------------------------------------------------------------------------------------------------------------------
    internal sealed class DredgerEventGenerator
    {
        // i.e. 1 in every 20 transactions is an anomaly
        private const int AnomalyFactor = 20;

        private const int MaxAmount = 1000;

        private const int MaxDays = 30;

        private readonly Random random = new Random((int)DateTimeOffset.UtcNow.Ticks);

        private readonly DateTimeOffset startTime = new DateTimeOffset(DateTime.Now);

        private readonly List<string> knownDredgers = new List<string>
        {
            "FC6018B3",
            "71B3B20F",
            "B3B19755",
            "1AF29CA0",
            "039AA17C",
        };

        private readonly List<string> knownDredgerNames = new List<string>
        {
            "Ellis Island",
            "Terrapin Island",
            "Key Largo",
            "Sanibel Island",
            "St. Augustine",
        };

        //--------------------------------------------------------------------------------------------------------------
        public IEnumerable<DredgerTelemetry> GenerateEvents(int count)
        {
            int counter = 0;

            var timestamp = startTime;

            while (counter < count)
            {
                foreach (var t in NewMockPurchases(timestamp))
                {
                    yield return t;
                    counter++;
                }

                timestamp += TimeSpan.FromMinutes(10);
            }
        }

        /// <summary>
        /// Returns a new mock transaction. In some cases where there is an anomaly, then it returns
        /// 2 transactions. one regular and one anomaly. They differ in amounts, locations and timestamps, but have
        /// the same credit card Id.
        /// At the consumer side, since the timestamps are close together but the locations are different, these
        /// 2 transactions are flagged as anomalous.
        /// </summary>
        /// <returns></returns>
        //--------------------------------------------------------------------------------------------------------------
        private IEnumerable<DredgerTelemetry> NewMockPurchases(DateTimeOffset timestamp)
        {
            var maxIndex = Math.Min(knownDredgers.Count, knownDredgerNames.Count);

            var index = random.Next(0, maxIndex);
            var id = knownDredgers[index];
            var name = knownDredgerNames[index];

            bool isAnomaly = (random.Next(0, AnomalyFactor) % AnomalyFactor) == 0;

            var purchases = new List<DredgerTelemetry>();

            var regularTransaction = new DredgerTelemetry
            {
                Data = new DredgerTelemetryData
                {
                    DREDGE_ID = id,
                    DREDGE_NAME = name,
                    DATE_TIME = timestamp,
                    LOAD_NUMBER = random.Next(1, MaxAmount)

                },
                Type = TelemetryType.Regular,
            };

            purchases.Add(regularTransaction);

            if (isAnomaly)
            {
                // change the location to something else
                // now the transaction on a credit card is happening from a different location which is an anomaly!

                string newName = null;

                do
                {
                    var newIndex = random.Next(0, knownDredgerNames.Count);
                    newName = knownDredgerNames[newIndex];

                    // while loop is - if by chance the "random" new location is the same as the original location

                } while (string.Equals(newName, name, StringComparison.OrdinalIgnoreCase));

                var suspectTransaction = new DredgerTelemetry
                {
                    Data = new DredgerTelemetryData
                    {
                        DREDGE_ID = id,
                        DREDGE_NAME = newName,
                        DATE_TIME = timestamp + TimeSpan.FromSeconds(2), // suspect transaction time range is close to a regular transaction
                        LOAD_NUMBER = random.Next(1, MaxAmount) 
                    },
                    Type = TelemetryType.Suspect,
                };

                purchases.Add(suspectTransaction);
            }

            return purchases;
        }
    }
}