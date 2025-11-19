using SmartAttendance.Models;

namespace SmartAttendance.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<IEnumerable<Attendance>> GetAttendanceByStudentIdAsync(int studentId);
        Task<IEnumerable<Attendance>> GetAttendanceBySubjectIdAsync(int subjectId);
        Task<IEnumerable<Attendance>> GetAllAsync();
        Task<Attendance> AddAttendanceAsync(Attendance attendance);
        Task<Attendance> UpdateAttendanceAsync(int StudentId, int SubjectId, Attendance attendance);
    Task<Attendance?> DeleteAttendanceAsync(int attendanceId);
        
    }
}
