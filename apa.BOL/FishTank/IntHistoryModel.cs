using System;

namespace apa.BOL.FishTank
{
    //--------------------------------------------------------------------------------------------------------------
    public class IntHistoryModel
    {
        public IntHistoryModel(DateTime timeStamp, int value)
        {
            TimeStamp = timeStamp;
            Value = value;
        }
        public DateTime TimeStamp { get; set; }
        public int Value { get; set; }
    }
}
