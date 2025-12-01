using UserService.Application.DTO;

namespace UserService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<int> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task ConfirmEmailAsync(int userId, string token);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task<int?> GetUserIdByEmailAsync(string email);
    }
}