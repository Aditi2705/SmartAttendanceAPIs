namespace SmartAttendance.DTOs.Attendance
{
    public class AttendanceRecordDto
    {
        public int StudentId { get; set; }
        public string? StudentName {  get; set; }
        public string? Year { get; set; }
        public string? Semester { get; set; }
        public bool IsPresent { get; set; } = default!;

    }
}
