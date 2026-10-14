using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    [Table("USER")]
    public class User
    {
        [Key]
        public int UserID { get; set; }
      
        public string Name { get; set; } = string.Empty;
       
        public string Surname { get; set; } = string.Empty;
       
        public string Email { get; set; } = string.Empty;
        
        public string PasswordHash { get; set; } = string.Empty;
       
        public string Role { get; set; } = string.Empty;

        // Relationships
        public ICollection<Enrollment> Enrollments { get; set; }
            = new List<Enrollment>();

        public ICollection<Event> OrganisedEvents { get; set; }
            = new List<Event>();
    }
}