using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using UserService.Application.DTO;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Infrastructure.Repositories;
namespace UserService.Application.Services
{

    // im tried boss...
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository; // Используем интерфейс репозитория
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            user.Name = dto.Name;
            user.Email = dto.Email;

            await _userRepository.UpdateAsync(user);

            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return false;

            await _userRepository.DeleteAsync(user);
            return true;
        }

        public async Task DeactivateAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new Exception("User not found");

            user.Deactivate();
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User {UserId} deactivated. Deactivating their products...", id);

            try
            {
                var productServiceUrl = _configuration["ProductService:BaseUrl"];
                var request = new HttpRequestMessage(HttpMethod.Patch, $"{productServiceUrl}/products/deactivate-by-user/{id}");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully deactivated products for user {UserId}", id);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to deactivate products for user {UserId}. Status: {StatusCode}, Error: {Error}",
                        id, response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling ProductService to deactivate products for user {UserId}", id);
            }
            await DeactivateUserProductsAsync(id);
        }

        private async Task DeactivateUserProductsAsync(int userId)
        {
            try
            {
                var productServiceUrl = _configuration["ApiSettings:ProductServiceUrl"];
                if (string.IsNullOrEmpty(productServiceUrl))
                {
                    _logger.LogWarning("ProductService URL not configured");
                    return;
                }

                var endpoint = $"{productServiceUrl}/products/deactivate-by-user/{userId}";
                _logger.LogInformation("Deactivating products for user {UserId} via ProductService: {Endpoint}", userId, endpoint);
                var response = await _httpClient.PatchAsync(endpoint, null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully deactivated products for user {UserId}", userId);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to deactivate products for user {UserId}. Status: {Status}, Error: {Error}", userId, response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating products for user {UserId}", userId);
            }
        }

        public async Task ActivateAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new Exception("User not found");

            user.Activate();
            await _userRepository.UpdateAsync(user);
        }

        public async Task<int?> GetUserIdByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user?.Id;
        }

        public async Task SetRoleAsync(int id, UserRoles role)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new Exception("User not found");

            user.SetRole(role);
            await _userRepository.UpdateAsync(user);
        }
    }
}
