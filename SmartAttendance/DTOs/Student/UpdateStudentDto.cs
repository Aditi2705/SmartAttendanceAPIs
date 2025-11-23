using System.ComponentModel.DataAnnotations;

namespace SmartAttendance.DTOs.Student
{
    public class UpdateStudentDto
    {
        public int Id { get; set; }

        public string? FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; } = string.Empty;
        public string? RollNumber { get; set; } = string.Empty;
        public string? ClassName { get; set; } = string.Empty;
    }
}
