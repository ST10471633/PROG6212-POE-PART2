using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentID { get; set; }

        [Required]
        public DateTime EnrollmentDate { get; set; }

        // Foreign keys
        [ForeignKey(nameof(Participant))]
        public int ParticipantID { get; set; }

        [ForeignKey(nameof(Event))]
        public int EventID { get; set; }

        [ForeignKey(nameof(Category))]
        public int CategoryID { get; set; }

        // Relationships
        public User? Participant { get; set; }

        public Event? Event { get; set; }

        public Category? Category { get; set; }

        public Result? Result { get; set; }
    }
}