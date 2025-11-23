using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SmartAttendance.Models;

namespace SmartAttendance.Controllers
{
    [ApiController]
    [Route("api/dev")]
    // NOTE: This controller is temporary and intended only to bootstrap an Admin user.
    // Remove this file immediately after creating your admin account.
    public class DevSeedController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DevSeedController> _logger;

        public DevSeedController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<DevSeedController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "UserName, Email and Password are required" });

            // Ensure Admin role exists
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("Admin"));
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to create Admin role: {errors}", roleResult.Errors);
                    return StatusCode(500, new { message = "Failed to create Admin role" });
                }
            }

            // Check user existence
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                return BadRequest(new { message = "User with this email already exists" });

            var user = new AppUser
            {
                UserName = dto.UserName.ToLower(),
                Email = dto.Email,
                FullName = dto.FullName ?? dto.UserName,
                Role = "Admin"
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                _logger.LogError("Failed to create admin user: {errors}", createResult.Errors);
                return StatusCode(500, createResult.Errors);
            }

            var addRole = await _userManager.AddToRoleAsync(user, "Admin");
            if (!addRole.Succeeded)
            {
                _logger.LogError("Failed to assign Admin role: {errors}", addRole.Errors);
                return StatusCode(500, addRole.Errors);
            }

            return Ok(new { message = "Admin user created", user = new { user.UserName, user.Email } });
        }

        public class CreateAdminDto
        {
            public string? FullName { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
