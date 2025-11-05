using SmartAttendance.Models;

namespace SmartAttendance.Interfaces
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAll();
        Task<Student?> GetById(int id);
        Task<Student> Add(Student student);
        Task<Student> Update(Student student);
        Task<Student> Delete (int id);
        Task<bool> SaveChangesAsync();
    }
}
