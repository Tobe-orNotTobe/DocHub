using Azure;
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
	[Authorize(AuthenticationSchemes = "Bearer")]
	public class AppointmentController : ControllerBase
	{
		private readonly APIResponse _response;
		private readonly IAppointmentService _appointmentService;

		public AppointmentController(APIResponse response, IAppointmentService appointmentService)
		{
			_response = response;
			_appointmentService = appointmentService;
		}

		/// <summary>
		/// Tạo lịch hẹn mới
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDTO createAppointmentDTO)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var result = await _appointmentService.CreateAppointmentAsync(currentUserId, createAppointmentDTO);

				_response.Result = result;
				_response.StatusCode = HttpStatusCode.Created;
				_response.IsSuccess = true;

				return CreatedAtAction(nameof(GetAppointmentById), new { appointmentId = result.AppointmentId }, _response);
			}
			catch (ArgumentException ex)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return BadRequest(_response);
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
		/// Lấy thông tin lịch hẹn theo ID
		/// </summary>
		[HttpGet("{appointmentId}")]
		public async Task<IActionResult> GetAppointmentById(int appointmentId)
		{
			try
			{
				var result = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
				if (result == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy lịch hẹn.");
					return NotFound(_response);
				}

				_response.Result = result;
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
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
		/// Lấy danh sách lịch hẹn theo ID người dùng
		/// </summary>
		[HttpGet("user/{userId}")]
		public async Task<IActionResult> GetAppointmentsByUserId(string userId)
		{
			try
			{
				var result = await _appointmentService.GetAppointmentsByUserIdAsync(userId);
				_response.Result = result;
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
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
		/// Lấy danh sách lịch hẹn theo ID bác sĩ
		/// </summary>
		[HttpGet("doctor/{doctorId}")]
		public async Task<IActionResult> GetAppointmentsByDoctorId(int doctorId)
		{
			try
			{
				var result = await _appointmentService.GetAppointmentsByDoctorIdAsync(doctorId);
				_response.Result = result;
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
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
		/// Lấy lịch hẹn sắp tới theo ID người dùng
		/// </summary>
		[HttpGet("user/{userId}/upcoming")]
		public async Task<IActionResult> GetUpcomingAppointments(string userId)
		{
			try
			{
				var result = await _appointmentService.GetUpcomingAppointmentsAsync(userId);
				_response.Result = result;
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
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

		[HttpPut("{appointmentId}")]
		public async Task<IActionResult> UpdateAppointment(int appointmentId, [FromBody] UpdateAppointmentDTO dto)
		{
			try
			{
				if (!ModelState.IsValid) return BadRequest(ModelState);
				var result = await _appointmentService.UpdateAppointmentAsync(appointmentId, dto);
				_response.Result = result;
				_response.IsSuccess = true;
				_response.StatusCode = HttpStatusCode.OK;
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
		/// Hủy lịch hẹn
		/// </summary>
		[HttpPost("{appointmentId}/cancel")]
		public async Task<IActionResult> CancelAppointment(int appointmentId, [FromBody] CancelAppointmentDTO dto)
		{
			try
			{
				if (!ModelState.IsValid) return BadRequest(ModelState);
				var result = await _appointmentService.CancelAppointmentAsync(appointmentId, dto);
				if (!result)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy lịch hẹn để hủy.");
					return NotFound(_response);
				}
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = "Hủy lịch hẹn thành công";
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
		/// Xác nhận lịch hẹn
		/// </summary>
		[HttpPost("{appointmentId}/confirm")]
		public async Task<IActionResult> ConfirmAppointment(int appointmentId)
		{
			try
			{
				var result = await _appointmentService.ConfirmAppointmentAsync(appointmentId);
				if (!result)
				{
					_response.IsSuccess = false;
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.ErrorMessages.Add("Không thể xác nhận lịch hẹn. Lịch hẹn không tồn tại hoặc không ở trạng thái chờ.");
					return BadRequest(_response);
				}
				_response.IsSuccess = true;
				_response.StatusCode = HttpStatusCode.OK;
				_response.Result = "Xác nhận lịch hẹn thành công";
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
		/// Hoàn tất lịch hẹn
		/// </summary>
		[HttpPost("{appointmentId}/complete")]
		public async Task<IActionResult> CompleteAppointment(int appointmentId)
		{
			try
			{
				var result = await _appointmentService.CompleteAppointmentAsync(appointmentId);
				if (!result)
				{
					_response.IsSuccess = false;
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.ErrorMessages.Add("Không thể hoàn tất lịch hẹn. Lịch hẹn không tồn tại hoặc đã bị hủy.");
					return BadRequest(_response);
				}
				_response.IsSuccess = true;
				_response.StatusCode = HttpStatusCode.OK;
				_response.Result = "Hoàn tất lịch hẹn thành công";
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
		/// Lấy lịch hẹn hôm nay
		/// </summary>
		[HttpGet("today")]
		public async Task<IActionResult> GetTodaysAppointments()
		{
			try
			{
				var result = await _appointmentService.GetTodaysAppointmentsAsync();
				_response.Result = result;
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
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
		/// Lấy lịch hẹn theo trạng thái
		/// </summary>
		[HttpGet("status/{status}")]
		public async Task<IActionResult> GetAppointmentsByStatus(string status)
		{
			try
			{
				var validStatuses = new[] { "pending", "confirmed", "completed", "cancelled", "paid" };
				if (!validStatuses.Contains(status.ToLower()))
					return BadRequest("Trạng thái không hợp lệ. Các trạng thái hợp lệ là: pending, confirmed, completed, cancelled, paid");

				var result = await _appointmentService.GetAppointmentsByStatusAsync(status.ToLower());
				_response.Result = result;
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
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