using Azure;
using DochubSystem.Common.Helper;
using DochubSystem.Data.DTOs;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DochubSystem.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
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

				var result = await _appointmentService.CreateAppointmentAsync(createAppointmentDTO);

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
	}
}