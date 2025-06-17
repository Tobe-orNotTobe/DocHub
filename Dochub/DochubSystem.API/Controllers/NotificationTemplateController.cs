using DochubSystem.Common.Helper;
using DochubSystem.Data.DTOs;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DochubSystem.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
	public class NotificationTemplateController : ControllerBase
	{
		private readonly INotificationTemplateService _templateService;
		private readonly APIResponse _response;

		public NotificationTemplateController(INotificationTemplateService templateService, APIResponse response)
		{
			_templateService = templateService;
			_response = response;
		}

		/// <summary>
		/// Lấy danh sách tất cả templates
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> GetAllTemplates()
		{
			try
			{
				var templates = await _templateService.GetAllTemplatesAsync();

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = templates;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Lỗi hệ thống: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Lấy template theo ID
		/// </summary>
		[HttpGet("{templateId}")]
		public async Task<IActionResult> GetTemplateById(int templateId)
		{
			try
			{
				var template = await _templateService.GetTemplateByIdAsync(templateId);

				if (template == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy template");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = template;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Lỗi hệ thống: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Lấy template theo type
		/// </summary>
		[HttpGet("type/{type}")]
		public async Task<IActionResult> GetTemplateByType(string type)
		{
			try
			{
				var template = await _templateService.GetTemplateByTypeAsync(type);

				if (template == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add($"Không tìm thấy template cho type: {type}");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = template;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Lỗi hệ thống: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Lấy templates theo target role
		/// </summary>
		[HttpGet("role/{role}")]
		public async Task<IActionResult> GetTemplatesByRole(string role)
		{
			try
			{
				var templates = await _templateService.GetTemplatesByRoleAsync(role);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = templates;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Lỗi hệ thống: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Tạo template mới
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> CreateTemplate([FromBody] CreateNotificationTemplateDTO createTemplateDTO)
		{
			try
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

				var template = await _templateService.CreateTemplateAsync(createTemplateDTO);

				_response.StatusCode = HttpStatusCode.Created;
				_response.IsSuccess = true;
				_response.Result = template;
				return CreatedAtAction(nameof(GetTemplateById), new { templateId = template.TemplateId }, _response);
			}
			catch (InvalidOperationException ex)
			{
				_response.StatusCode = HttpStatusCode.Conflict;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return Conflict(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Lỗi hệ thống: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Cập nhật template
		/// </summary>
		[HttpPut("{templateId}")]
		public async Task<IActionResult> UpdateTemplate(int templateId, [FromBody] UpdateNotificationTemplateDTO updateTemplateDTO)
		{
			try
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

				var template = await _templateService.UpdateTemplateAsync(templateId, updateTemplateDTO);

				if (template == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy template");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = template;
				return Ok(_response);
			}
			catch (ArgumentException ex)
			{
				_response.StatusCode = HttpStatusCode.NotFound;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return NotFound(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Lỗi hệ thống: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Xóa template
		/// </summary>
		[HttpDelete("{templateId}")]
		public async Task<IActionResult> DeleteTemplate(int templateId)
		{
			try
			{
				var result = await _templateService.DeleteTemplateAsync(templateId);

				if (!result)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy template");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = "Đã xóa template thành công" };
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Lỗi hệ thống: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Kích hoạt template
		/// </summary>
		[HttpPut("{templateId}/activate")]
		public async Task<IActionResult> ActivateTemplate(int templateId)
		{
			try
			{
				var result = await _templateService.ActivateTemplateAsync(templateId);

				if (!result)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy template");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = "Đã kích hoạt template thành công" };
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Lỗi hệ thống: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Vô hiệu hóa template
		/// </summary>
		[HttpPut("{templateId}/deactivate")]
		public async Task<IActionResult> DeactivateTemplate(int templateId)
		{
			try
			{
				var result = await _templateService.DeactivateTemplateAsync(templateId);

				if (!result)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy template");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = "Đã vô hiệu hóa template thành công" };
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Lỗi hệ thống: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Seed default templates
		/// </summary>
		[HttpPost("seed-defaults")]
		public async Task<IActionResult> SeedDefaultTemplates()
		{
			try
			{
				await _templateService.SeedDefaultTemplatesAsync();

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = "Đã seed default templates thành công" };
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Lỗi hệ thống: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Kiểm tra template type đã tồn tại
		/// </summary>
		[HttpGet("exists/{type}")]
		public async Task<IActionResult> CheckTemplateExists(string type)
		{
			try
			{
				var exists = await _templateService.ExistsByTypeAsync(type);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Type = type, Exists = exists };
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Lỗi hệ thống: {ex.Message}");
				return StatusCode(500, _response);
			}
		}
	}
}