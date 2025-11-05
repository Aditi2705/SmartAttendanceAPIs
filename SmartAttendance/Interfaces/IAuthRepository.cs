using Microsoft.AspNetCore.Identity;
using SmartAttendance.Dtos.Account;


namespace SmartAttendance.Interfaces
{
    public interface IAuthRepository
    {
        Task<IdentityResult> RegisterAsync(RegisterDto dto);
        Task<string?> LoginAsync(LoginDto dto);
    }
}
