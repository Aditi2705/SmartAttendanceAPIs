namespace SmartAttendance.DTOs.Admin
{
    public class BulkUploadRequest
    {
        public string Mode { get; set; } // "addNew" or "override"
        public List<BulkUploadStudentDto> Students { get; set; }
    }

    public class BulkUploadStudentDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string RollNo { get; set; }
        public string ClassName { get; set; }
        public string Password { get; set; } = "Pass@1234"; // Default password
    }
}
