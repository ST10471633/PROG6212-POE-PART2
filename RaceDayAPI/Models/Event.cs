using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

      
        public string EventName { get; set; } = string.Empty;

     
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

       
        public string Location { get; set; } = string.Empty;

        [Required]
        public decimal Distance { get; set; }

        // Foreign keys
        [ForeignKey(nameof(EventType))]
        public int EventTypeID { get; set; }

        [ForeignKey(nameof(Organiser))]
        public int OrganiserID { get; set; }

        // Relationships
        public EventType? EventType { get; set; }

        public User? Organiser { get; set; }

        public ICollection<Category> Categories { get; set; }
            = new List<Category>();

        public ICollection<Enrollment> Enrollments { get; set; }
            = new List<Enrollment>();
    }
}