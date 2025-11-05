using SmartAttendance.Models;

namespace SmartAttendance.DTOs.Admin
{
    public class GetSubjectDto
    {
        public int Id { get; set; }
        public string SubjectName { get; set; } = default!;
        public string SubjectCode { get; set; } = default!;
        public string ClassName { get; set; } = default!;
        public string? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
        public ICollection<SmartAttendance.Models.Attendance> Attendances { get; set; }
    }
}
