using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Data;
using SmartAttendance.Interfaces;
using SmartAttendance.Models;

namespace SmartAttendance.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public async Task<Student> Add(Student student)
        {
            await _context.Students.AddAsync(student);
            // Don't save here - let caller manage SaveChanges
            return student;
        }

        public async Task<Student?> Delete(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                // Don't save here - let caller manage SaveChanges
            }
            return student;
        }

        public async Task<IEnumerable<Student>> GetAll()
        {
            return await _context.Students.ToListAsync();
        }

        public async Task<Student?> GetById(int id)
        {
            return await _context.Students.FindAsync(id);
        }

        public async Task<Student?> GetByRollNoAsync(string rollNo)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.RollNo == rollNo);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Student> Update(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
            return student;
        }
    }
}
