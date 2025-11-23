using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.DTOs.Teacher;
using SmartAttendance.Interfaces;
using SmartAttendance.Models;

namespace SmartAttendance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherRepository _teacherRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public TeacherController(ITeacherRepository teacherRepo, UserManager<AppUser> userManager, IMapper mapper)
        {
            _teacherRepo = teacherRepo;
            _userManager = userManager;
            _mapper = mapper;
        }

        // GET api/teacher/me - returns the teacher record for the logged-in user
        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GetMe()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var teacher = await _teacherRepo.GetTeacherByUserId(user.Id);
            if (teacher == null) return NotFound();
            return Ok(_mapper.Map<CreateTeacherDto>(teacher));
        }

        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var teacher = await _teacherRepo.GetAllTeachers();
            return Ok(_mapper.Map<IEnumerable<CreateTeacherDto>>(teacher));
        }

        [HttpGet("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var teacher = await _teacherRepo.GetTeacherById(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<CreateTeacherDto>(teacher));
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTeacher(CreateTeacherDto teacherDto)
        {
            // ✅ Step 1: Create AppUser
            var user = new AppUser
            {
                FullName = teacherDto.FullName,
                Email = teacherDto.Email,
                UserName = teacherDto.Email, // username = email
            };

            var result = await _userManager.CreateAsync(user, teacherDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            //Assign role
            await _userManager.AddToRoleAsync(user, "Teacher");

            // ✅ Step 2: Map DTO → Teacher and link UserId
            var teacher = _mapper.Map<Teacher>(teacherDto);
            teacher.UserId = user.Id; // Link Teacher with AppUser

            await _teacherRepo.AddTeacher(teacher);
            await _teacherRepo.SaveChangesAsync();

            return Ok(new
            {
                message = "Teacher created successfully",
                teacher = _mapper.Map<CreateTeacherDto>(teacher)
            });
        }

        [HttpPut("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTeacher(int id, UpdateTeacherDto teacherDto)
        {
            var teacher = await _teacherRepo.GetTeacherById(id);
            if (teacher == null)
            {
                return NotFound();
            }
            _mapper.Map(teacherDto, teacher);
            await _teacherRepo.UpdateTeacher(teacher);
            await _teacherRepo.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var teacher = await _teacherRepo.GetTeacherById(id);
            if (teacher == null) return NotFound();

            await _teacherRepo.DeleteTeacher(id);
            await _teacherRepo.SaveChangesAsync();

            return NoContent();
        }
    }
}
