namespace SmartAttendance.DTOs.Student
{
    public class SubjectAttendanceDto
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = default!;
        public int TotalSessions { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount => TotalSessions - PresentCount;
        public double Percentage { get; set; }
    }
}
