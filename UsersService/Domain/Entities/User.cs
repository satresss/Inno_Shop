using System.ComponentModel.DataAnnotations;
using System.Data;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }   
        public string Email { get; set; }   
        public string PasswordHash { get; set; }

        public UserRoles Role { get; set; } = UserRoles.User;
        public bool IsActive { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTimeOffset? PasswordResetTokenExpires { get; set; }
        public string? RefreshToken { get; set; }
        public DateTimeOffset? RefreshTokenExpiryTime { get; set; }
        public void SetPasswordHash(string hash)
        {
            PasswordHash = hash;
        }

        public void SetEmailConfirmationToken(string token)
        {
            EmailConfirmationToken = token;
        }

        public void ConfirmEmail()
        {
            EmailConfirmationToken = null;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void SetRole(UserRoles role)
        {
            Role = role;
        }

        public void SetPasswordResetToken(string token, DateTimeOffset expires)
        {
            PasswordResetToken = token;
            PasswordResetTokenExpires = expires;
        }

        public void ClearPasswordResetToken()
        {
            PasswordResetToken = null;
            PasswordResetTokenExpires = null;
        }

        public User(string name, string email, string passwordHash)
        {
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
        }
        public void SetRefreshToken(string token, DateTimeOffset expires)
        {
            RefreshToken = token;
            RefreshTokenExpiryTime = expires;
        }
    }
}
