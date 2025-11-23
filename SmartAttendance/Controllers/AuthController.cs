using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Dtos.Account;
using SmartAttendance.Interfaces;
using SmartAttendance.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartAttendance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IStudentRepository _studentRepo;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IStudentRepository studentRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _studentRepo = studentRepo;
        }


        // ✅ REGISTER API
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if user already exists
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == registerDto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Email already registered." });

            // Create user object
            // NOTE: For security, public registration will only create Student accounts.
            // If you need to create an Admin or Teacher account, use the admin-only flow
            // (create via Admin API or create account manually), see documentation.
            var roleToAssign = "Student"; // force Student for public registration

            var appUser = new AppUser
            {
                FullName = registerDto.FullName,
                UserName = registerDto.UserName.ToLower(),
                Email = registerDto.Email,
                Role = roleToAssign
            };

            var result = await _userManager.CreateAsync(appUser, registerDto.Password);

            if (!result.Succeeded)
                return StatusCode(500, result.Errors);

            // Assign role (forced to Student for public registration)
            var roleResult = await _userManager.AddToRoleAsync(appUser, roleToAssign);
            if (!roleResult.Succeeded)
                return StatusCode(500, roleResult.Errors);

            // Generate token (pass assigned role so token contains correct role claim)
            var token = _tokenService.CreateToken(appUser, new[] { roleToAssign });

            return Ok(new
            {
                message = "User registered successfully.",
                user = new
                {
                    fullName = appUser.FullName,
                    email = appUser.Email,
                    username = appUser.UserName,
                    role = appUser.Role
                },
                token
            });
        }

        // ✅ LOGIN API
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find user by username
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.UserName.ToLower());

            if (user == null)
                return Unauthorized(new { message = "Invalid username or user does not exist." });

            // Validate password
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Incorrect password." });

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate token (pass roles so token contains accurate role claims)
            var token = _tokenService.CreateToken(user, roles);

            return Ok(new
            {
                message = "Login successful.",
                user = new
                {
                    fullName = user.FullName,
                    email = user.Email,
                    username = user.UserName,
                    roles = roles
                },
                token
            });
        }

        // LOGIN by student roll number (convenience endpoint used by frontend)
        [HttpPost("students/login")]
        public async Task<IActionResult> LoginByRollNo([FromBody] LoginByRollDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.RollNo) || string.IsNullOrEmpty(dto.Password))
                return BadRequest(new { message = "rollNo and password required" });

            // Find student by roll no
            var student = await _studentRepo.GetByRollNoAsync(dto.RollNo);
            if (student == null)
                return Unauthorized(new { message = "Invalid roll number or user not found." });

            // Find associated AppUser
            var user = await _userManager.FindByIdAsync(student.UserId);
            if (user == null)
                return Unauthorized(new { message = "User not found for this student." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Incorrect password." });

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);
            return Ok(new
            {
                message = "Login successful.",
                user = new { fullName = user.FullName, email = user.Email, username = user.UserName, role = user.Role },
                token
            });
        }
    }
}
