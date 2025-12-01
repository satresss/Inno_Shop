using BCrypt.Net;
using UserService.Application.DTO;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository; // Репозиторий
        private readonly IJwtProvider _jwtProvider;
        private readonly IEmailService _emailService;

        public AuthService(
            IUserRepository userRepository,
            IJwtProvider jwtProvider,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _emailService = emailService;
        }

        public async Task<int> RegisterAsync(RegisterDto dto)
        {
            if (!await _userRepository.IsEmailUniqueAsync(dto.Email))
                throw new Exception("Email already exists");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = new User(dto.Name, dto.Email, passwordHash);

            var token = Guid.NewGuid().ToString();
            user.SetEmailConfirmationToken(token);

            await _userRepository.AddAsync(user);

            var confirmationLink = $"https://localhost:5035/Login?confirmEmail=true&userId={user.Id}&token={token}";
            var emailBody = $@"
            <h2>Подтвердите ваш email</h2>
            <p>Здравствуйте, {dto.Name}!</p>
            <p>Для завершения регистрации нажмите на ссылку ниже:</p>
            <p><a href='{confirmationLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Подтвердить email</a></p>
            <p>Или скопируйте эту ссылку в браузер:<br>
            <a href='{confirmationLink}'>{confirmationLink}</a></p>
            <p><strong>Токен подтверждения:</strong> {token}</p>
            ";

            await _emailService.SendEmailAsync(dto.Email, "Подтверждение регистрации", emailBody);

            return user.Id;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception("Invalid email or password");

            if (!user.IsActive && user.EmailConfirmationToken != null)
                throw new Exception("Email not confirmed");

            var accessToken = _jwtProvider.GenerateAccessToken(user);
            var refreshToken = _jwtProvider.GenerateRefreshToken();

            user.SetRefreshToken(refreshToken, DateTimeOffset.UtcNow.AddDays(7));
            await _userRepository.UpdateAsync(user);

            return new AuthResponseDto { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);

            if (user == null || user.RefreshTokenExpiryTime < DateTimeOffset.UtcNow)
            {
                throw new Exception("Invalid token");
            }

            var newAccess = _jwtProvider.GenerateAccessToken(user);
            var newRefresh = _jwtProvider.GenerateRefreshToken();

            user.SetRefreshToken(newRefresh, DateTimeOffset.UtcNow.AddDays(7));
            await _userRepository.UpdateAsync(user);

            return new AuthResponseDto { AccessToken = newAccess, RefreshToken = newRefresh };
        }

        public async Task ConfirmEmailAsync(int userId, string token)
        {
            var user = await _userRepository.GetByEmailConfirmationTokenAsync(token);

            if (user == null || user.Id != userId)
                throw new Exception("Invalid token");

            user.ConfirmEmail();
            user.Activate();
            await _userRepository.UpdateAsync(user);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return;

            var token = Guid.NewGuid().ToString();
            user.SetPasswordResetToken(token, DateTimeOffset.UtcNow.AddHours(1));
            await _userRepository.UpdateAsync(user);

            await _emailService.SendEmailAsync(email, "Reset Password", $"Token: {token}");
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userRepository.GetByPasswordResetTokenAsync(dto.Token, dto.Email);

            if (user == null || user.PasswordResetTokenExpires < DateTimeOffset.UtcNow)
                throw new Exception("Invalid or expired token");

            var newHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.SetPasswordHash(newHash);
            user.ClearPasswordResetToken();

            await _userRepository.UpdateAsync(user);
        }

        public async Task<int?> GetUserIdByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user?.Id;
        }
    }
}
