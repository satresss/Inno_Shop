using UserService.Application.DTO;
using UserService.Domain.Enums;

namespace UserService.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(int id);
        Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto);
        Task<bool> DeleteAsync(int id);
        Task DeactivateAsync(int id);
        Task ActivateAsync(int id);
        Task<int?> GetUserIdByEmailAsync(string email);
        Task SetRoleAsync(int id, UserRoles role);
    }
}
