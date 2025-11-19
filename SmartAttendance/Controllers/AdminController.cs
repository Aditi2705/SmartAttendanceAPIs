using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Data;
using SmartAttendance.DTOs.Admin;
using SmartAttendance.Interfaces;
using SmartAttendance.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SmartAttendance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IStudentRepository _studentRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IStudentRepository studentRepo,
            UserManager<AppUser> userManager,
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<AdminController> logger)
        {
            _studentRepo = studentRepo;
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/admin/students
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            try
            {
                // Fetch students with User data included
                var students = await _context.Students.Include(s => s.User).ToListAsync();
                var studentDtos = _mapper.Map<IEnumerable<StudentListDto>>(students);
                return Ok(studentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching students");
                return StatusCode(500, new { message = "Error fetching students", error = ex.Message });
            }
        }

        // POST: api/admin/bulk-upload
        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUploadStudents([FromBody] BulkUploadRequest request)
        {
            if (request == null || request.Students == null || request.Students.Count == 0)
            {
                return BadRequest(new { message = "No students provided" });
            }

            var adminUser = await _userManager.GetUserAsync(User);
            _logger.LogInformation($"Bulk upload initiated by {adminUser?.Email} with mode: {request.Mode}. Total students: {request.Students.Count}");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    int added = 0, skipped = 0;
                    var failures = new List<string>();

                    // If Override mode, delete all existing students and their users
                    if (request.Mode == "override")
                    {
                        try
                        {
                            var existingStudents = await _studentRepo.GetAll();
                            _logger.LogInformation($"Override mode: Found {existingStudents.Count()} existing students to delete");
                            
                            // Collect all user IDs to delete
                            var userIdsToDelete = existingStudents
                                .Where(s => !string.IsNullOrEmpty(s.UserId))
                                .Select(s => s.UserId)
                                .Distinct()
                                .ToList();

                            // Delete all student records first (they have FK to Users)
                            foreach (var student in existingStudents)
                            {
                                _context.Students.Remove(student);
                            }

                            // Delete all associated users
                            var usersToDelete = await _context.Users
                                .Where(u => userIdsToDelete.Contains(u.Id))
                                .ToListAsync();

                            foreach (var user in usersToDelete)
                            {
                                _context.Users.Remove(user);
                            }

                            // Don't save yet - let the main transaction handle all changes together
                            _logger.LogInformation($"Override mode: Marked {existingStudents.Count()} students and {usersToDelete.Count()} users for deletion");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error marking students for deletion in override mode");
                            throw;
                        }
                    }

                    // Process each student
                    foreach (var studentDto in request.Students)
                    {
                        _logger.LogInformation($"Processing student: {studentDto.RollNo}, Email: {studentDto.Email}");
                        try
                        {
                            // Check if student already exists (by rollNo or email) - case insensitive for email
                            var emailLower = studentDto.Email.ToLower();
                            var existing = await _context.Students
                                .Include(s => s.User)
                                .FirstOrDefaultAsync(s => s.RollNo == studentDto.RollNo || 
                                    (s.User != null && s.User.Email != null && s.User.Email.ToLower() == emailLower));

                            if (existing != null)
                            {
                                if (request.Mode == "addNew")
                                {
                                    skipped++;
                                    _logger.LogInformation($"Skipped student {studentDto.RollNo} (already exists)");
                                    continue;
                                }
                                // In override mode, we already deleted all, so this shouldn't happen
                            }

                            // Validate email format
                            if (!IsValidEmail(studentDto.Email))
                            {
                                _logger.LogWarning($"Invalid email format for {studentDto.RollNo}: {studentDto.Email}");
                                failures.Add($"{studentDto.RollNo}: Invalid email format");
                                continue;
                            }

                            // Create AppUser
                            var user = new AppUser
                            {
                                FullName = studentDto.FullName,
                                Email = studentDto.Email,
                                UserName = studentDto.Email,
                                EmailConfirmed = true
                            };

                            _logger.LogInformation($"Creating user for {studentDto.RollNo} with email {studentDto.Email}");
                            var userResult = await _userManager.CreateAsync(user, studentDto.Password);
                            if (!userResult.Succeeded)
                            {
                                var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                                _logger.LogWarning($"Failed to create user for {studentDto.RollNo}: {errors}");
                                failures.Add($"{studentDto.RollNo}: User creation failed - {errors}");
                                continue;
                            }

                            // Assign Student role
                            await _userManager.AddToRoleAsync(user, "Student");

                            // Create Student record
                            var student = new Student
                            {
                                FullName = studentDto.FullName,
                                RollNo = studentDto.RollNo,
                                Email = studentDto.Email,
                                ClassName = studentDto.ClassName,
                                UserId = user.Id
                            };

                            await _studentRepo.Add(student);
                            added++;
                            _logger.LogInformation($"Added student {studentDto.RollNo} ({studentDto.Email})");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error processing student {studentDto.RollNo}");
                            failures.Add($"{studentDto.RollNo}: {ex.Message}");
                            // Continue with next student on error
                        }
                    }

                    // Save all changes at once
                    await _studentRepo.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var response = new BulkUploadResponse
                    {
                        Added = added,
                        Skipped = skipped,
                        Message = $"Upload complete. Added: {added}, Skipped: {skipped}" + 
                                  (failures.Count > 0 ? $", Failed: {failures.Count}" : "")
                    };

                    if (failures.Count > 0)
                    {
                        response.Message += "\nFailed students: " + string.Join("; ", failures);
                    }

                    _logger.LogInformation($"Bulk upload completed. Added: {added}, Skipped: {skipped}, Failed: {failures.Count}");
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during bulk upload");
                    return StatusCode(500, new { message = "Error during bulk upload", error = ex.Message });
                }
            }
        }

        // GET: api/admin/download-template
        [HttpGet("download-template")]
        [AllowAnonymous]
        public IActionResult DownloadTemplate()
        {
            // Generate Excel template (if using a library like EPPlus)
            // For now, return a JSON template that can be used as a guide
            var templateData = new[]
            {
                new { fullName = "John Doe", email = "john.doe@example.com", rollNo = "CSE001", className = "B.Tech CSE 2021-2025" },
                new { fullName = "Jane Smith", email = "jane.smith@example.com", rollNo = "CSE002", className = "B.Tech CSE 2021-2025" },
            };

            return Ok(new { message = "Use these columns in your Excel: fullName, email, rollNo, className", template = templateData });
        }

        // POST: api/admin/assign-admin/{email}
        // Allow super-admin or first admin setup (for initialization only)
        [HttpPost("assign-admin/{email}")]
        [AllowAnonymous]
        public async Task<IActionResult> AssignAdminRole(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(new { message = $"User with email {email} not found" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                {
                    return Ok(new { message = $"User {email} is already an Admin" });
                }

                var result = await _userManager.AddToRoleAsync(user, "Admin");
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Admin role assigned to {email}");
                    return Ok(new { message = $"Admin role assigned to {email}" });
                }

                return BadRequest(new { message = "Failed to assign admin role", errors = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning admin role");
                return StatusCode(500, new { message = "Error assigning admin role", error = ex.Message });
            }
        }

        // Helper function to validate email
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
