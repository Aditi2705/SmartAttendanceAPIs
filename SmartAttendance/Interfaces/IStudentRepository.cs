using SmartAttendance.Models;

namespace SmartAttendance.Interfaces
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAll();
        Task<Student?> GetById(int id);
        Task<Student?> GetByRollNoAsync(string rollNo);
        Task<Student> Add(Student student);
        Task<Student> Update(Student student);
    Task<Student?> Delete (int id);
        Task<bool> SaveChangesAsync();
    }
}
