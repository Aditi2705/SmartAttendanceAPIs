using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAttendance.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string SubjectName { get; set; } = default!;
        public string SubjectCode { get; set; } = default!;
        public string ClassName { get; set; } = default!;
        public string? TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public Teacher? Teacher { get; set; }
        public ICollection<Attendance> Attendances { get; set; }

    }
}
