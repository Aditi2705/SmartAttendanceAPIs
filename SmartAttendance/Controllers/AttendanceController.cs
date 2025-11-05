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
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceRepository _attendanceRepo;
        private readonly IMapper _mapper;
        public AttendanceController(IAttendanceRepository attendanceRepo, IMapper mapper)
        {
            _attendanceRepo = attendanceRepo;
            _mapper = mapper;
        }

        [HttpGet("{Studentid}")]
        public async Task<IActionResult> GetByStudentId(int StudentId)
        {
            var subjects = await _attendanceRepo.GetAttendanceByStudentIdAsync(StudentId);
            var result = _mapper.Map<IEnumerable<AttendanceRecordDto>>(subjects);
            return Ok(result);
        }

        [HttpGet("{Subjectid}")]
        public async Task<IActionResult> GetBySubjectId(int SubjectId)
        {
            var subjects = await _attendanceRepo.GetAttendanceBySubjectIdAsync(SubjectId);
            var result = _mapper.Map<IEnumerable<AttendanceRecordDto>>(subjects);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddAttendance(AttendanceCreateDto create)
        {
            var attendance = _mapper.Map<Attendance>(create);
            await _attendanceRepo.AddAttendanceAsync(attendance);
            return Ok(_mapper.Map<AttendanceCreateDto>(attendance));
        }
        [HttpPut("{StudentId}")]
        public async Task<IActionResult> UpdateAttendance(int StudentId, int SubjectId, AttendanceUpdateDto update)
        {
            var student = await _attendanceRepo.GetAttendanceByStudentIdAsync(StudentId);
            if (student == null)
                return NotFound();
            var subject = await _attendanceRepo.GetAttendanceBySubjectIdAsync(SubjectId);
            if (subject == null)
                return NotFound();
            var attendance = _mapper.Map<Attendance>(update);
            await _attendanceRepo.UpdateAttendanceAsync(attendance);
            return Ok(_mapper.Map<AttendanceUpdateDto>(attendance));
        }
    }
}
