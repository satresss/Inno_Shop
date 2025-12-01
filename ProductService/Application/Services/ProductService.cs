using AutoMapper;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTO;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository, 
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            _logger.LogInformation("Getting all products");
            var products = await _productRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Getting product by ID: {ProductId}", id);
            var product = await _productRepository.GetByIdAsync(id);
            
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return null;
            }

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetByUserIdAsync(int userId)
        {
            _logger.LogInformation("Getting products for user ID: {UserId}", userId);
            var products = await _productRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> SearchAsync(ProductFilterDto filter)
        {
            _logger.LogInformation(
                "Searching products with filters: SearchTerm={SearchTerm}, MinPrice={MinPrice}, MaxPrice={MaxPrice}, IsAvailable={IsAvailable}, PageNumber={PageNumber}, PageSize={PageSize}",
                filter.SearchTerm, filter.MinPrice, filter.MaxPrice, filter.IsAvailable, filter.PageNumber, filter.PageSize);

            if (filter.PageNumber < 1)
                filter.PageNumber = 1;
            
            if (filter.PageSize < 1 || filter.PageSize > 100)
                filter.PageSize = 10;

            if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue)
            {
                if (filter.MinPrice.Value > filter.MaxPrice.Value)
                {
                    throw new ArgumentException("MinPrice cannot be greater than MaxPrice");
                }
            }

            var products = await _productRepository.SearchAsync(
                filter.SearchTerm,
                filter.MinPrice,
                filter.MaxPrice,
                filter.IsAvailable,
                filter.CreatedByUserId,
                filter.PageNumber,
                filter.PageSize
            );
            
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> CreateAsync(CreateProductDto dto, int userId)
        {
            _logger.LogInformation("Creating product for user ID: {UserId}", userId);

            var product = new Product(dto.Name, dto.Description, dto.Price, userId);
            product.SetAvailability(dto.IsAvailable);
            
            await _productRepository.AddAsync(product);
            
            _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto, int userId)
        {
            _logger.LogInformation("Updating product ID: {ProductId} by user ID: {UserId}", id, userId);

            var product = await _productRepository.GetByIdAsync(id);
            
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                throw new ProductNotFoundException(id);
            }

            if (product.CreatedByUserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to update product {ProductId} owned by user {OwnerId}",
                    userId, id, product.CreatedByUserId);
                throw new UnauthorizedProductAccessException(id, userId);
            }

            product.Update(dto.Name, dto.Description, dto.Price, dto.IsAvailable);
            await _productRepository.UpdateAsync(product);

            _logger.LogInformation("Product {ProductId} updated successfully", id);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            _logger.LogInformation("Deleting product ID: {ProductId} by user ID: {UserId}", id, userId);

            var product = await _productRepository.GetByIdAsync(id);
            
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                throw new ProductNotFoundException(id);
            }

            if (product.CreatedByUserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to delete product {ProductId} owned by user {OwnerId}",
                    userId, id, product.CreatedByUserId);
                throw new UnauthorizedProductAccessException(id, userId);
            }

            await _productRepository.DeleteAsync(product);
            _logger.LogInformation("Product {ProductId} deleted successfully", id);
            return true;
        }

        public async Task<bool> IsProductOwnerAsync(int productId, int userId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            return product != null && product.CreatedByUserId == userId;
        }

        public async Task DeactivateByUserIdAsync(int userId)
        {
            _logger.LogInformation("Deactivating all products for user ID: {UserId}", userId);
            await _productRepository.DeactivateByUserIdAsync(userId);
            _logger.LogInformation("Successfully deactivated all products for user ID: {UserId}", userId);
        }
    }
}

