using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartAttendance.Models
{
    public class Student
    {
        public int Id { get; set; }

        public string UserId { get; set; } = default!;
        public AppUser? User { get; set; }

        [Required]
        public string FullName { get; set; } = default!;

        [Required]
        public string RollNo { get; set; } = default!;

        [Required]
        public string Email { get; set; } = default!;

        [Required]
        public string ClassName { get; set; } = default!;

        public List<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}
 