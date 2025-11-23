using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.DTOs.Student;
using SmartAttendance.Interfaces;
using SmartAttendance.Models;
using SmartAttendance.Repositories;

namespace SmartAttendance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class StudentController : Controller
    {

        private readonly IStudentRepository _studentRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public StudentController(IStudentRepository studentRepo, UserManager<AppUser> userManager, IMapper mapper)
        {
            _studentRepo = studentRepo;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            var student = await _studentRepo.GetAll();
            return Ok(_mapper.Map<IEnumerable<CreateStudentDto>>(student));

        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            // Get the logged-in user from the JWT claims
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // Find the student linked to this user
            var students = await _studentRepo.GetAll();
            var student = students.FirstOrDefault(s => s.UserId == userId);

            if (student == null)
            {
                return NotFound(new { message = "Student profile not found" });
            }

            return Ok(_mapper.Map<CreateStudentDto>(student));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _studentRepo.GetById(id);
            if (student == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<CreateStudentDto>(student));
        }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> CreateStudent(CreateStudentDto studentDto)
        {
            // ✅ Step 1: Create AppUser
            var user = new AppUser
            {
                FullName = studentDto.FullName,
                Email = studentDto.Email,
                UserName = studentDto.Email, // username = email
            };

            var result = await _userManager.CreateAsync(user, studentDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // ✅ Step 2: Map DTO → Student and link UserId
            var student = _mapper.Map<Student>(studentDto);
            student.UserId = user.Id; // Link Student with AppUser

            await _studentRepo.Add(student);
            await _studentRepo.SaveChangesAsync();

            return Ok(new
            {
                message = "Student created successfully",
                student = _mapper.Map<CreateStudentDto>(student)
            });
        }

            [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateStudentDto dto)
        {
            var student = await _studentRepo.GetById(id);
            if (student == null)
            {
                return NotFound();
            }
            _mapper.Map(dto, student);
            await _studentRepo.Update(student);
            await _studentRepo.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _studentRepo.GetById(id);
            if (student == null) return NotFound();

            await _studentRepo.Delete(id);
            await _studentRepo.SaveChangesAsync();

            return NoContent();
        }

    }
}
