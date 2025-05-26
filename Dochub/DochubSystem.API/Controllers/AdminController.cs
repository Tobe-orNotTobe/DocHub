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
		private readonly APIResponse _response;

		public AdminController(IAdminService adminService, APIResponse response)
		{
			_adminService = adminService;
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

			_response.StatusCode = HttpStatusCode.OK;
			_response.IsSuccess = true;
			_response.Result = new { Message = result.Message, UserId = result.UserId };
			return Ok(_response);
		}

		
	}
}
