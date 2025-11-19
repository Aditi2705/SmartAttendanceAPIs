using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.DTOs.Admin;
using SmartAttendance.DTOs.Attendance;
using SmartAttendance.Interfaces;
using SmartAttendance.Models;
using SmartAttendance.Repositories;

namespace SmartAttendance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceRepository _attendanceRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly IMapper _mapper;
        public AttendanceController(IAttendanceRepository attendanceRepo, IStudentRepository studentRepo, IMapper mapper)
        {
            _attendanceRepo = attendanceRepo;
            _studentRepo = studentRepo;
            _mapper = mapper;
        }
        // GET by student id
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudentId(int studentId)
        {
            var records = await _attendanceRepo.GetAttendanceByStudentIdAsync(studentId);
            var result = _mapper.Map<IEnumerable<AttendanceRecordDto>>(records);
            return Ok(result);
        }

        // GET by subject id
        [HttpGet("subject/{subjectId}")]
        public async Task<IActionResult> GetBySubjectId(int subjectId)
        {
            var records = await _attendanceRepo.GetAttendanceBySubjectIdAsync(subjectId);
            var result = _mapper.Map<IEnumerable<AttendanceRecordDto>>(records);
            return Ok(result);
        }

        // POST single attendance record
        [HttpPost]
        public async Task<IActionResult> AddAttendance([FromBody] Attendance attendance)
        {
            var created = await _attendanceRepo.AddAttendanceAsync(attendance);
            return Ok(created);
        }

        // POST a full session (many attendance records)
        [HttpPost("session")]
        public async Task<IActionResult> AddAttendanceSession([FromBody] AttendanceSessionDto session)
        {
            if (session == null || session.Attendance == null) return BadRequest();
            var created = new List<Attendance>();
            foreach (var row in session.Attendance)
            {
                // Try to resolve student by roll number
                var student = string.IsNullOrEmpty(row.RollNo) ? null : await _studentRepo.GetByRollNoAsync(row.RollNo!);
                var attendance = new Attendance
                {
                    StudentId = student?.Id ?? 0,
                    StudentName = row.Name ?? student?.FullName,
                    SubjectName = session.Subject,
                    CourseName = session.Course,
                    Semester = int.TryParse(session.Semester, out var sem) ? sem : 0,
                    Date = session.Date,
                    IsPresent = (row.Status ?? "").ToUpper() == "P"
                };
                var added = await _attendanceRepo.AddAttendanceAsync(attendance);
                created.Add(added);
            }
            return Ok(new { message = "Session saved", count = created.Count, records = created });
        }

        // GET history grouped by session (date+subject+course)
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var all = (await _attendanceRepo.GetAllAsync()).ToList();
            // group by date + subject + course
            var groups = all.GroupBy(a => new { a.Date.Date, a.SubjectName, a.CourseName })
                .Select(g => new {
                    id = g.Key.Date.ToString("yyyyMMdd") + "_" + (g.Key.SubjectName ?? ""),
                    date = g.Key.Date,
                    subject = g.Key.SubjectName,
                    course = g.Key.CourseName,
                    attendance = g.Select(a => new { rollNo = a.StudentName, name = a.StudentName, status = a.IsPresent ? "P" : "A" }).ToList(),
                    _present = g.Count(x => x.IsPresent),
                    _total = g.Count(),
                    _percent = g.Count() == 0 ? 0 : Math.Round((double)g.Count(x => x.IsPresent) / g.Count() * 100, 1)
                })
                .OrderByDescending(x => x.date)
                .ToList();

            return Ok(groups);
        }

        // PUT update attendance by id
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendance(int id, [FromBody] AttendanceUpdateDto update)
        {
            // find existing attendance record
            var existing = (await _attendanceRepo.GetAttendanceByStudentIdAsync(update.AttendanceId)).FirstOrDefault();
            if (existing == null) return NotFound();
            existing.IsPresent = update.IsPresent;
            var updated = await _attendanceRepo.UpdateAttendanceAsync(existing.StudentId, existing.SubjectId, existing);
            return Ok(updated);
        }
    }
}
