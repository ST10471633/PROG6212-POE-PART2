using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }
     
        public string CategoryName { get; set; } = string.Empty;
 
        public string CategoryType { get; set; } = string.Empty;

        // Foreign key
        [ForeignKey(nameof(Event))]
        public int EventID { get; set; }

        // Relationship
        public Event? Event { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }
            = new List<Enrollment>();
    }
}