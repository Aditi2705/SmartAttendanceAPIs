using SmartAttendance.Models;

namespace SmartAttendance.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
