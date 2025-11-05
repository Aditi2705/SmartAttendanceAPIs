using SmartAttendance.Models;

namespace SmartAttendance.Interfaces
{
    public interface ITeacherRepository
    {
        Task<IEnumerable<Teacher>> GetAllTeachers();
        Task<Teacher?> GetTeacherById(int id);
        Task <Teacher> AddTeacher(Teacher teacher);
        Task <Teacher> UpdateTeacher(Teacher teacher);
        Task <Teacher> DeleteTeacher(int id);
        Task <bool> SaveChangesAsync();
    }
}
