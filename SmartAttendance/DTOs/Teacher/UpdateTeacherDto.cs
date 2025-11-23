using System.ComponentModel.DataAnnotations;

namespace SmartAttendance.DTOs.Teacher
{
    public class UpdateTeacherDto
    {
        public string? FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
        public string? TeacherId { get; set; } = string.Empty;
        public string? Department { get; set; } = string.Empty;
    }
}
