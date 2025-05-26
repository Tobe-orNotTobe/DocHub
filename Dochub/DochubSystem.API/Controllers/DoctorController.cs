using DochubSystem.Common.Helper;
using DochubSystem.Data.DTOs;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DochubSystem.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DoctorController : ControllerBase
	{
		private readonly APIResponse _response;
		private readonly IDoctorService _doctorService;

		public DoctorController(APIResponse response, IDoctorService doctorService)
		{
			_response = response;
			_doctorService = doctorService;
		}

		/// <summary>
		/// Tạo bác sĩ mới
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorDTO createDoctorDTO)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var result = await _doctorService.CreateDoctorAsync(createDoctorDTO);

				_response.Result = result; 
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;

				return CreatedAtAction(nameof(GetDoctorById),
					new { doctorId = result.DoctorId }, _response);
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
				_response.ErrorMessages.Add($"Internal server error: {ex.Message}");

				return StatusCode((int)HttpStatusCode.InternalServerError, _response);
			}
		}

		/// <summary>
		/// Lấy thông tin bác sĩ theo ID
		/// </summary>
		[HttpGet("{doctorId}")]
		public async Task<IActionResult> GetDoctorById(int doctorId)
		{
			try
			{
				var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
				if (doctor == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy bác sĩ.");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = doctor;
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