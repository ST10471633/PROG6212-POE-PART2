using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Result
    {
        [Key]
        public int ResultsID { get; set; }

        public string? FinishTime { get; set; }

        public int? FinishPosition { get; set; }
     
        public string Status { get; set; } = string.Empty;

        // Foreign key
        [ForeignKey(nameof(Enrollment))]
        public int EnrollmentID { get; set; }

        // Relationship
        public Enrollment? Enrollment { get; set; }
    }
}