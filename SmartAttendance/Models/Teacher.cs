using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAttendance.Models
{
    public class Teacher
    {
        [Key]
        public string? TeacherId { get; set; }

        public string FullName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Password { get; set; }
        public string UserId { get; set; } = default!;
        public AppUser? User { get; set; }

        
        public string? Department { get; set; }

        [JsonIgnore]
        public List<Subject> Subjects { get; set; } = new List<Subject>();
    }
}
