using SmartAttendance.Models;

namespace SmartAttendance.DTOs.Subject
{
    public class CreateSubjectDto
    {
        public string SubjectName { get; set; } = default!;
        public string SubjectCode { get; set; } = default!;
        public string ClassName { get; set; } = default!;
        public string? TeacherId { get; set; }
    }
}
