using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.DTOs.Attendance;
using SmartAttendance.Interfaces;
using SmartAttendance.Models;
using SmartAttendance.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace SmartAttendance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceRepository _attendanceRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly ISubjectRepository _subjectRepo;
        private readonly IMapper _mapper;
        public AttendanceController(IAttendanceRepository attendanceRepo, IStudentRepository studentRepo, ISubjectRepository subjectRepo, IMapper mapper)
        {
            _attendanceRepo = attendanceRepo;
            _studentRepo = studentRepo;
            _subjectRepo = subjectRepo;
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
            // fetch subjects once to avoid repeated DB calls
            var knownSubjects = (await _subjectRepo.GetAllAsync()).ToList();
            foreach (var row in session.Attendance)
            {
                // Try to resolve student by roll number
                var student = string.IsNullOrEmpty(row.RollNo) ? null : await _studentRepo.GetByRollNoAsync(row.RollNo!);
                // derive semester number (try to extract first integer from session.Semester)
                int sem = 0;
                if (!string.IsNullOrEmpty(session.Semester)){
                    var m = Regex.Match(session.Semester, "(\\d+)");
                    if(m.Success) int.TryParse(m.Value, out sem);
                }
                // derive year from batch (e.g. "2021-2025" -> 2021)
                int year = 0;
                if(!string.IsNullOrEmpty(session.Batch)){
                    var my = Regex.Match(session.Batch, "(\\d{4})");
                    if(my.Success) int.TryParse(my.Value, out year);
                }

                // Build a class name using Course + Batch (e.g. "B.Tech CSE 2021-2025") and store it in CourseName
                var className = string.IsNullOrWhiteSpace(session.Batch) ? session.Course : (string.IsNullOrWhiteSpace(session.Course) ? session.Batch : session.Course + " " + session.Batch);

                // resolve subject id (by subject name). If not found, create it.
                int subjectId = 0;
                if (!string.IsNullOrWhiteSpace(session.Subject))
                {
                    var found = knownSubjects.FirstOrDefault(s => string.Equals(s.SubjectName, session.Subject, StringComparison.OrdinalIgnoreCase));
                    if (found != null)
                    {
                        subjectId = found.Id;
                    }
                    else
                    {
                        var newSub = new SmartAttendance.Models.Subject { SubjectName = session.Subject ?? "Unknown", ClassName = className ?? string.Empty };
                        var createdSub = await _subjectRepo.AddAsync(newSub);
                        subjectId = createdSub.Id;
                        // keep local cache in sync
                        knownSubjects.Add(createdSub);
                    }
                }

                var attendance = new Attendance
                {
                    StudentId = student?.Id ?? 0,
                    StudentName = row.Name ?? student?.FullName,
                    SubjectId = subjectId,
                    SubjectName = session.Subject,
                    CourseName = className,
                    Semester = sem,
                    Year = year,
                    Date = session.Date,
                    IsPresent = (row.Status ?? "").ToUpper() == "P"
                };
                var added = await _attendanceRepo.AddAttendanceAsync(attendance);
                created.Add(added);
            }
            var resultDtos = _mapper.Map<IEnumerable<SmartAttendance.DTOs.Attendance.AttendanceRecordDto>>(created);
            return Ok(new { message = "Session saved", count = created.Count, records = resultDtos });
        }

        // GET history grouped by session (date+subject+course)
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var all = (await _attendanceRepo.GetAllAsync()).ToList();
            // group by full DateTime + subject + course so separate sessions on same day are not merged
            var groups = all.GroupBy(a => new { a.Date, a.SubjectName, a.CourseName, a.Semester })
                .Select(g => new {
                    id = g.Key.Date.ToString("yyyyMMddHHmmss") + "_" + (g.Key.SubjectName ?? "") + "_" + (g.Key.Semester),
                    date = g.Key.Date.ToString("o"),
                    subject = g.Key.SubjectName,
                    course = g.Key.CourseName,
                    semester = g.Key.Semester,
                    // Use Student.RollNo (from navigation property) if available, otherwise fall back to StudentName
                    attendance = g.Select(a => new { 
                        rollNo = a.Student?.RollNo ?? a.StudentName ?? "", 
                        name = a.StudentName, 
                        status = a.IsPresent ? "P" : "A" 
                    }).ToList(),
                    _present = g.Count(x => x.IsPresent),
                    _total = g.Count(),
                    _percent = g.Count() == 0 ? 0 : Math.Round((double)g.Count(x => x.IsPresent) / g.Count() * 100, 1)
                })
                .OrderByDescending(x => x.date)
                .ToList();

            return Ok(groups);
        }

        // Temporary public endpoint for debugging: return history without requiring auth
        [AllowAnonymous]
        [HttpGet("history/public")]
        public async Task<IActionResult> GetHistoryPublic()
        {
            var all = (await _attendanceRepo.GetAllAsync()).ToList();
            var groups = all.GroupBy(a => new { Date = a.Date, a.SubjectName, a.CourseName, a.Semester })
                .Select(g => new {
                    id = g.Key.Date.ToString("yyyyMMddHHmmss") + "_" + (g.Key.SubjectName ?? "") + "_" + (g.Key.Semester),
                    date = g.Key.Date.ToString("o"), // ISO string
                    subject = g.Key.SubjectName,
                    course = g.Key.CourseName,
                    semester = g.Key.Semester,
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
