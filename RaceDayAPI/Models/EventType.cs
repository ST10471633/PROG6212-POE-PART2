using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeID { get; set; }

     
        public string TypeName { get; set; } = string.Empty;

        // Relationship
        public ICollection<Event> Events { get; set; }
            = new List<Event>();
    }
}