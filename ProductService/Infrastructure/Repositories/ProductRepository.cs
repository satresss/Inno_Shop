using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _context;

        public ProductRepository(ProductDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<IEnumerable<Product>> GetByUserIdAsync(int userId)
        {
            return await _context.Products
                .Where(p => p.CreatedByUserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchAsync(string? searchTerm, decimal? minPrice, decimal? maxPrice, bool? isAvailable, int? createdByUserId, int pageNumber, int pageSize)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchTermLower = searchTerm.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchTermLower) || 
                    p.Description.ToLower().Contains(searchTermLower));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (isAvailable.HasValue)
            {
                query = query.Where(p => p.IsAvailable == isAvailable.Value);
            }

            if (createdByUserId.HasValue)
            {
                query = query.Where(p => p.CreatedByUserId == createdByUserId.Value);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

        public async Task DeactivateByUserIdAsync(int userId)
        {
            var products = await _context.Products
                .Where(p => p.CreatedByUserId == userId)
                .ToListAsync();

            foreach (var product in products)
            {
                product.IsAvailable = false;
            }

            await _context.SaveChangesAsync();
        }
    }
}

