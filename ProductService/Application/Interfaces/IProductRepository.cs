using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Product>> SearchAsync(string? searchTerm, decimal? minPrice, decimal? maxPrice, bool? isAvailable, int? createdByUserId, int pageNumber, int pageSize);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product);
        Task<bool> ExistsAsync(int id);
        Task DeactivateByUserIdAsync(int userId);
    }
}

