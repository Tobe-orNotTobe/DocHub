using DochubSystem.Common.Helper;
using DochubSystem.Data.DTOs;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace DochubSystem.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
	public class AdminController : ControllerBase
	{
		private readonly IAdminService _adminService;
		private readonly IDoctorService _doctorService; 
		private readonly APIResponse _response;

		public AdminController(IAdminService adminService, IDoctorService doctorService, APIResponse response)
		{
			_adminService = adminService;
			_doctorService = doctorService;
			_response = response;
		}

		/// <summary>
		/// Tạo tài khoản mới cho người dùng.
		/// </summary>
		[HttpPost("create-account")]
		public async Task<IActionResult> CreateAccount([FromBody] RegisterAccountDTO model)
		{
			if (!ModelState.IsValid)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages = ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage)
					.ToList();
				return BadRequest(_response);
			}

			var result = await _adminService.CreateAccountAsync(model);

			if (!result.Success)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages = result.Errors;
				return BadRequest(_response);
			}

			try
			{
				var createDoctorDTO = new CreateDoctorDTO
				{
					UserId = result.UserId,
					IsActive = true
				};

				var doctorResult = await _doctorService.CreateDoctorAsync(createDoctorDTO);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new
				{
					Message = result.Message,
					UserId = result.UserId,
					DoctorId = doctorResult.DoctorId, 
					EmailSent = "Email chào mừng với thông tin đăng nhập đã được gửi tới địa chỉ email của người dùng."
				};
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages = new List<string> { $"Tạo tài khoản thành công nhưng tạo thông tin bác sĩ thất bại: {ex.Message}" };
				return BadRequest(_response);
			}
		}

		/// <summary>
		/// Lấy danh sách tất cả người dùng kèm vai trò.
		/// </summary>
		[HttpGet("getAllUsers")]
		public async Task<IActionResult> GetAllUsers()
		{
			var users = await _adminService.GetAllUsersWithRolesAsync();

			_response.StatusCode = HttpStatusCode.OK;
			_response.IsSuccess = true;
			_response.Result = users;
			return Ok(_response);
		}

		/// <summary>
		/// Lấy thông tin người dùng theo Id.
		/// </summary>
		[HttpGet("getUserById/{id}")]
		public async Task<IActionResult> GetUserById(string id)
		{
			var userData = await _adminService.GetUserByIdWithRolesAsync(id);

			if (userData == null)
			{
				_response.StatusCode = HttpStatusCode.NotFound;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("Không tìm thấy người dùng");
				return NotFound(_response);
			}

			_response.StatusCode = HttpStatusCode.OK;
			_response.IsSuccess = true;
			_response.Result = userData;
			return Ok(_response);
		}

		/// <summary>
		/// Xóa người dùng theo Id.
		/// </summary>
		[HttpDelete("deleteUser/{id}")]
		public async Task<IActionResult> DeleteUser(string id)
		{
			var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var result = await _adminService.DeleteUserAsync(id, currentUserId);

			if (!result.Success)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages = result.Errors;
				return BadRequest(_response);
			}

			_response.StatusCode = HttpStatusCode.OK;
			_response.IsSuccess = true;
			_response.Result = result.Message;
			return Ok(_response);
		}

		/// <summary>
		/// Cập nhật thông tin người dùng.
		/// </summary>
		[HttpPut("updateUser")]
		public async Task<IActionResult> UpdateUser([FromBody] UserDTO model)
		{
			var result = await _adminService.UpdateUserAsync(model);

			if (!result.Success)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages = result.Errors;
				return BadRequest(_response);
			}

			_response.StatusCode = HttpStatusCode.OK;
			_response.IsSuccess = true;
			_response.Result = result.Message;
			return Ok(_response);
		}

		/// <summary>
		/// Kích hoạt tài khoản người dùng theo Id.
		/// </summary>
		[HttpPut("activate/{id}")]
		public async Task<IActionResult> ActivateUser(string id)
		{
			var result = await _adminService.ActivateUserAsync(id);

			if (!result.Success)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages = result.Errors;
				return BadRequest(_response);
			}

			_response.StatusCode = HttpStatusCode.OK;
			_response.IsSuccess = true;
			_response.Result = result.Message;
			return Ok(_response);
		}

		/// <summary>
		/// Vô hiệu hóa tài khoản người dùng theo Id.
		/// </summary>
		[HttpPut("deactivate/{id}")]
		public async Task<IActionResult> DeactivateUser(string id)
		{
			var result = await _adminService.DeactivateUserAsync(id);

			if (!result.Success)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages = result.Errors;
				return BadRequest(_response);
			}

			_response.StatusCode = HttpStatusCode.OK;
			_response.IsSuccess = true;
			_response.Result = result.Message;
			return Ok(_response);
		}

		/// <summary>
		/// Lấy danh sách tất cả các vai trò (Roles).
		/// </summary>
		[HttpGet("getAllRoles")]
		public async Task<IActionResult> GetAllRoles()
		{
			var roles = await _adminService.GetAllRolesAsync();

			_response.StatusCode = HttpStatusCode.OK;
			_response.IsSuccess = true;
			_response.Result = roles;
			return Ok(_response);
		}

		/// <summary>
		/// Tạo vai trò (Role) mới.
		/// </summary>
		[HttpPost("createRole")]
		public async Task<IActionResult> CreateRole([FromBody] string roleName)
		{
			var result = await _adminService.CreateRoleAsync(roleName);

			if (!result.Success)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages = result.Errors;
				return BadRequest(_response);
			}

			_response.StatusCode = HttpStatusCode.OK;
			_response.IsSuccess = true;
			_response.Result = result.Message;
			return Ok(_response);
		}
	}
}
