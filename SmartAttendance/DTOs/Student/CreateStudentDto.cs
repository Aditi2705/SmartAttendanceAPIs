using SmartAttendance.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartAttendance.DTOs.Student
{
    public class CreateStudentDto
    {
        [Required]
        public string FullName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; } = "Student@123";

        [Required]
        public string RollNo { get; set; }

        [Required]
        public string ClassName { get; set; }
    }
}
