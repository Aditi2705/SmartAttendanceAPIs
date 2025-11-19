namespace SmartAttendance.DTOs.Attendance
{
    public class AttendanceSessionRecordDto
    {
        public string? RollNo { get; set; }
        public string? Name { get; set; }
        public string? Status { get; set; } // "P" or "A"
    }

    public class AttendanceSessionDto
    {
        public string? Course { get; set; }
        public string? Batch { get; set; }
        public string? Semester { get; set; }
        public string? Subject { get; set; }
        public DateTime Date { get; set; }
        public List<AttendanceSessionRecordDto> Attendance { get; set; } = new();
    }
}
