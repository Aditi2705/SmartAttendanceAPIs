using System.ComponentModel.DataAnnotations;

namespace SmartAttendance.DTOs.Teacher
{
    public class CreateTeacherDto
    {
        [Required]
        public string FullName { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; }

        [Required]
        public string TeacherId { get; set; } = default!;

        [Required]
        public string Department { get; set; } = default!;
    }
}
