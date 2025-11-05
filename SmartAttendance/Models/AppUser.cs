using Microsoft.AspNetCore.Identity;

namespace SmartAttendance.Models
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; } = string.Empty;
        public string? Role { get; set; } = string.Empty;
    }
}
