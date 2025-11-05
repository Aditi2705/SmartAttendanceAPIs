namespace SmartAttendance.DTOs.Student
{
    public class TotalAttendanceDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = default!;
        public int TotalSessions { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent => TotalSessions - TotalPresent;
        public double OverallPercentage { get; set; }

        public List<SubjectAttendanceDto> SubjectWise { get; set; } = new();
    }
}
