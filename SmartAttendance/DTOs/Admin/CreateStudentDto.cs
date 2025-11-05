using SmartAttendance.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartAttendance.DTOs.Admin
{
    public class CreateStudentDto
    {
     

        [Required]
        public string FullName { get; set; } 

        [EmailAddress]
        public string Email { get; set; } 

        [Required]
        public string Password { get; set; } = "Student@123"; // default

        [Required]
        public string RollNo { get; set; } 

        [Required]
        public string ClassName { get; set; } 
    }
}
