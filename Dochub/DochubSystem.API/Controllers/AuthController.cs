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
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly APIResponse _response;

		public AuthController(IAuthService authService, APIResponse response)
		{
			_authService = authService;
			_response = response;
		}

		[AllowAnonymous]
		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] UserRegisterDTO dto)
		{
			try
			{
				var user = await _authService.RegisterAsync(dto);
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = "Đăng ký thành công. Vui lòng xác nhận email của bạn." };
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return BadRequest(_response);
			}
		}

		[HttpPost("confirm-email")]
		public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest model)
		{
			try
			{
				var result = await _authService.ConfirmEmailAsync(model.Email, model.Token);

				if (!result)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Token không hợp lệ hoặc đã hết hạn.");
					return BadRequest(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = "Email đã được xác nhận thành công." };
				return Ok(_response);
			}
			catch (Exception ex)
			{
				if (ex.Message == "Email đã được xác nhận.")
				{
					_response.StatusCode = HttpStatusCode.OK;
					_response.IsSuccess = false;
					_response.Result = new { Message = "Email đã được xác nhận." };
					return Ok(_response);
				}

				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return BadRequest(_response);
			}
		}

		[AllowAnonymous]
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
		{
			try
			{
				var result = await _authService.LoginAsync(loginRequestDTO);
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = result;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.NotFound;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return Unauthorized(_response);
			}
		}

		[AllowAnonymous]
		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO model)
		{
			if (string.IsNullOrWhiteSpace(model.RefreshToken))
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("Cần phải có Refresh token làm mới.");
				return BadRequest(_response);
			}

			try
			{
				var result = await _authService.RefreshTokenAsync(model.RefreshToken);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = result;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return BadRequest(_response);
			}
		}

		[AllowAnonymous]
		[HttpPost("forget-password")]
		public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequestDTO model)
		{
			if (string.IsNullOrWhiteSpace(model.Email))
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("Cần phải có Email.");
				return BadRequest(_response);
			}

			try
			{
				await _authService.ForgetPasswordAsync(model.Email);
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = "Đã gửi liên kết đặt lại mật khẩu." };
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return BadRequest(_response);
			}
		}

		[AllowAnonymous]
		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Cần phải nhập Email, Token và Mật khẩu mới.");
					return BadRequest(_response);
				}

				var (success, message) = await _authService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword);

				if (!success)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add(message);
					return BadRequest(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Success = true, Message = message };
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Lỗi hệ thống {ex.Message}");
				return BadRequest(_response);
			}
		}

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Vui lòng nhập đầy đủ mật khẩu cũ và mới.");
                    return BadRequest(_response);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    _response.StatusCode = HttpStatusCode.Unauthorized;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Không xác định được người dùng.");
                    return Unauthorized(_response);
                }

                var (success, message) = await _authService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
                if (!success)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add(message);
                    return BadRequest(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = new { Message = message };
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Lỗi hệ thống: " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }
    }
}
