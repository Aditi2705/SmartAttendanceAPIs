namespace SmartAttendance.DTOs.Student
{
    public class StudentListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string RollNo { get; set; } = default!;
        public string ClassName { get; set; } = default!;
    }
}
