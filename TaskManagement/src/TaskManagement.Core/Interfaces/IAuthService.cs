using TaskManagement.Core.DTO.Auth;

namespace TaskManagement.Core.Interfaces
{
    public interface IAuthService
    {
        Task ForgotPasswordAsync(string email);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task LogoutAsync(string refreshToken);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<Guid> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
}