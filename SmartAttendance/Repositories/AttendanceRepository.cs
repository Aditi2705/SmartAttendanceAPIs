using Microsoft.EntityFrameworkCore;
using SmartAttendance.Data;
using SmartAttendance.Interfaces;
using SmartAttendance.Models;

namespace SmartAttendance.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly ApplicationDbContext _context;
        public AttendanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Attendance> AddAttendanceAsync(Attendance attendance)
        {
           await _context.Attendances.AddAsync(attendance);
           await  _context.SaveChangesAsync();
            return attendance; 
        }

        public async Task<Attendance?> DeleteAttendanceAsync(int attendanceId)
        {
            var attendance = await _context.Attendances.FindAsync(attendanceId);
            if (attendance != null)
            {
                _context.Attendances.Remove(attendance);
                await _context.SaveChangesAsync();
            }
            return attendance;
        }

        public async Task<IEnumerable<Attendance>> GetAllAsync()
        {
            return await _context.Attendances.ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByStudentIdAsync(int studentId)
        {
            return await _context.Attendances
                .Where(a => a.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceBySubjectIdAsync(int subjectId)
        {
            return await _context.Attendances
                .Where(a => a.SubjectId == subjectId)
                .ToListAsync();
        }

        public async Task<Attendance> UpdateAttendanceAsync(int StudentId, int SubjectId, Attendance attendance)
        {
            _context.Attendances.Update(attendance);
            await _context.SaveChangesAsync();
            return attendance;
        }
    }
}
