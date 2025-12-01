using ProductService.Application.DTO;

namespace ProductService.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<ProductDto>> SearchAsync(ProductFilterDto filter);
        Task<ProductDto?> CreateAsync(CreateProductDto dto, int userId);
        Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto, int userId);
        Task<bool> DeleteAsync(int id, int userId);
        Task<bool> IsProductOwnerAsync(int productId, int userId);
        Task DeactivateByUserIdAsync(int userId);
    }
}

