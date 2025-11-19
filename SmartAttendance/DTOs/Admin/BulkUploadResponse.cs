namespace SmartAttendance.DTOs.Admin
{
    public class BulkUploadResponse
    {
        public int Added { get; set; }
        public int Skipped { get; set; }
        public string Message { get; set; }
    }
}
