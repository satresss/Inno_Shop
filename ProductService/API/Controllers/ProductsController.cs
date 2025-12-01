using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTO;
using ProductService.Application.Interfaces;
using System.Security.Claims;

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllAsync()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        [HttpGet("my-products")]
        public async Task<IActionResult> GetMyProductsAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var products = await _productService.GetByUserIdAsync(userId.Value);
            return Ok(products);
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchAsync([FromQuery] ProductFilterDto filter)
        {
            var products = await _productService.SearchAsync(filter);
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateProductDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var product = await _productService.CreateAsync(dto, userId.Value);
            if (product == null)
                return BadRequest("Failed to create product");

            return CreatedAtAction(nameof(GetByIdAsync), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, UpdateProductDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                var updatedProduct = await _productService.UpdateAsync(id, dto, userId.Value);
                return Ok(updatedProduct);
            }
            catch (Application.Exceptions.ProductNotFoundException)
            {
                return NotFound();
            }
            catch (Application.Exceptions.UnauthorizedProductAccessException)
            {
                return Forbid();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                await _productService.DeleteAsync(id, userId.Value);
                return NoContent();
            }
            catch (Application.Exceptions.ProductNotFoundException)
            {
                return NotFound();
            }
            catch (Application.Exceptions.UnauthorizedProductAccessException)
            {
                return Forbid();
            }
        }

        [HttpPatch("deactivate-by-user/{userId}")]
        public async Task<IActionResult> DeactivateByUserIdAsync(int userId)
        {
            await _productService.DeactivateByUserIdAsync(userId);
            return Ok(new { message = $"All products for user {userId} have been deactivated" });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value
                           ?? User.FindFirst("userId")?.Value;

            if (int.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }
    }
}

