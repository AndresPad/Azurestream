using System;

namespace apa.BOL.Models
{
    //----------------------------------------------------------------------------------------------------------
    public class TodoModel
    {
        public string Id { get; set; }
        public string Group { get; set; }
        public string Content { get; set; }
        public string Due { get; set; }
        public bool Completed { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
