using UserService.Domain.Entities;

namespace UserService.Application.Interfaces
{
    public interface IUserRepository
    {
        // Базовые CRUD
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);

        // Специфичные методы для Auth и поиска
        Task<User?> GetByEmailAsync(string email);
        Task<bool> IsEmailUniqueAsync(string email);

        // Поиск по токенам
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task<User?> GetByEmailConfirmationTokenAsync(string token);
        Task<User?> GetByPasswordResetTokenAsync(string token, string email);
    }
}
