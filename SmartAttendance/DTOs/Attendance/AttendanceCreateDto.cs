namespace SmartAttendance.DTOs.Attendance
{
    public class AttendanceCreateDto
    {
        public int SubjectId { get; set; } = default!;
        public DateTime Date { get; set; }

        public List<AttendanceRecordDto> AttendanceRecords { get; set; } = new();

    }
}
