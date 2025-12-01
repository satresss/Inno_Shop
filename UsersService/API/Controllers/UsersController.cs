using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTO;
using UserService.Application.Interfaces;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id) 
        {
            var users = await _userService.GetByIdAsync(id);
            if (users == null) 
                return NotFound();
            return Ok(users); 
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, UpdateUserDto userDto)
        {
            var updatedUser = await _userService.UpdateAsync(id, userDto);
            if (updatedUser == null)
                return NotFound();
            return Ok(updatedUser);
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var success = await _userService.DeleteAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> DeactivateAsync(int id)
        {
            await _userService.DeactivateAsync(id);
            return Ok(new { message = "User deactivated" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> ActivateAsync(int id)
        {
            await _userService.ActivateAsync(id);
            return Ok(new { message = "User activated" });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("by-email")]
        public async Task<IActionResult> GetUserIdByEmail([FromQuery] string email)
        {
            var userId = await _userService.GetUserIdByEmailAsync(email);
            if (userId == null)
                return NotFound(new { message = "User not found" });

            return Ok(new { userId = userId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/assign-admin")]
        public async Task<IActionResult> AssignAdminRole(int id)
        {
            await _userService.SetRoleAsync(id, Domain.Enums.UserRoles.Admin);
            return Ok(new { message = "Admin role assigned successfully" });
        }
    }
}
