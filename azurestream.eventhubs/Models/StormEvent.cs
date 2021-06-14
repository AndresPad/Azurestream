namespace azurestream.eventhubs.Models
{
    public class StormEvent
    {
        public string StartTime { set; get; }
        public string EndTime { set; get; }
        public int EpisodeId { set; get; }
        public int EventId { set; get; }
        public string State { set; get; }
        public string EventType { set; get; }
        public int InjuriesDirect { set; get; }
        public int InjuriesIndirect { set; get; }
        public int DeathsDirect { set; get; }
        public int DeathsIndirect { set; get; }
        public int DamageProperty { set; get; }
        public int DamageCrops { set; get; }
        public string Source { set; get; }
        public string BeginLocation { set; get; }
        public string EndLocation { set; get; }
        public string BeginLat { set; get; }
        public string BeginLon { set; get; }
        public string EndLat { set; get; }
        public string EndLon { set; get; }
        public string EpisodeNarrative { set; get; }
        public string EventNarrative { set; get; }
        public dynamic StormSummary { set; get; }
    }
}
