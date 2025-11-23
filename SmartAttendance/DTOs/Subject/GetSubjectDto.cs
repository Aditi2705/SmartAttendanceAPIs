using SmartAttendance.Models;

namespace SmartAttendance.DTOs.Subject
{
    public class GetSubjectDto
    {
        public int Id { get; set; }
        public string SubjectName { get; set; } = default!;
        public string SubjectCode { get; set; } = default!;
        public string ClassName { get; set; } = default!;
        public string? TeacherId { get; set; }
        public SmartAttendance.Models.Teacher? Teacher { get; set; }
        public System.Collections.Generic.ICollection<SmartAttendance.Models.Attendance> Attendances { get; set; }
    }
}
