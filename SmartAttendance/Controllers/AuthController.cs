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

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
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
            var appUser = new AppUser
            {
                FullName = registerDto.FullName,
                UserName = registerDto.UserName.ToLower(),
                Email = registerDto.Email,
                Role = registerDto.Role // "Admin", "Teacher", or "Student"
            };

            var result = await _userManager.CreateAsync(appUser, registerDto.Password);

            if (!result.Succeeded)
                return StatusCode(500, result.Errors);

            // Assign role
            var roleResult = await _userManager.AddToRoleAsync(appUser, registerDto.Role);
            if (!roleResult.Succeeded)
                return StatusCode(500, roleResult.Errors);

            // Generate token
            var token = _tokenService.CreateToken(appUser);

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

            // Generate token
            var token = _tokenService.CreateToken(user);

            return Ok(new
            {
                message = "Login successful.",
                user = new
                {
                    fullName = user.FullName,
                    email = user.Email,
                    username = user.UserName,
                    role = user.Role
                },
                token
            });
        }
    }
}
