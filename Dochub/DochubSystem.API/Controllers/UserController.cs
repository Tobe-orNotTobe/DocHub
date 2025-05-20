using DochubSystem.Data.DTOs;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DochubSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Tạo User mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO createUserDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.CreateUserAsync(createUserDTO);

            if (result.Succeeded)
            {
                return Ok("User created successfully.");
            }

            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Lấy danh sách User
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Lấy thông tin User theo ID
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound("User not found.");
            return Ok(user);
        }

        /// <summary>
        /// Cập nhật User
        /// </summary>
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserDTO updateUserDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.UpdateUserAsync(userId, updateUserDTO);

            if (result.Succeeded)
            {
                return Ok("User updated successfully.");
            }

            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Xóa User
        /// </summary>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _userService.DeleteUserAsync(userId);

            if (result.Succeeded)
            {
                return Ok("User deleted successfully.");
            }

            return BadRequest(result.Errors);
        }
    }
}
