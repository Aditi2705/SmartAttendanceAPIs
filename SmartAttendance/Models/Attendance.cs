using System;
using System.ComponentModel.DataAnnotations;

namespace SmartAttendance.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
    // Navigation property to Student (optional)
    public Student? Student { get; set; }
        

        [Required]
        public int SubjectId { get; set; }
        public string? SubjectName { get; set; }
    // Navigation property to Subject (optional)
    public Subject? Subject { get; set; }
        
        public string? CourseName { get; set; }
        public int Year { get; set; }
        public int Semester { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public bool IsPresent { get; set; }
    }
}
