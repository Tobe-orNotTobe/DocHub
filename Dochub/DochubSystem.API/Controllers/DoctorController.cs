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

		/// <summary>
		/// Lấy danh sách tất cả bác sĩ
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> GetAllDoctors()
		{
			try
			{
				var doctors = await _doctorService.GetAllDoctorsAsync();
				_response.Result = doctors;
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
		/// Lấy danh sách bác sĩ đang hoạt động
		/// </summary>
		[HttpGet("active")]
		public async Task<IActionResult> GetAllActiveDoctors()
		{
			try
			{
				var doctors = await _doctorService.GetAllActiveDoctorsAsync();
				_response.Result = doctors;
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
		/// Cập nhật thông tin bác sĩ
		/// </summary>
		[HttpPut("{doctorId}")]
		public async Task<IActionResult> UpdateDoctor(int doctorId, [FromBody] UpdateDoctorDTO updateDoctorDTO)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var result = await _doctorService.UpdateDoctorAsync(doctorId, updateDoctorDTO);
				_response.Result = result;
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;

				return Ok(_response);
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
		/// Xóa bác sĩ
		/// </summary>
		[HttpDelete("{doctorId}")]
		public async Task<IActionResult> DeleteDoctor(int doctorId)
		{
			try
			{
				var result = await _doctorService.DeleteDoctorAsync(doctorId);
				if (!result)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy bác sĩ.");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = "Xóa bác sĩ thành công.";
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
		/// Lấy thông tin bác sĩ theo UserId
		/// </summary>
		[HttpGet("user/{userId}")]
		public async Task<IActionResult> GetDoctorByUserId(string userId)
		{
			try
			{
				var doctor = await _doctorService.GetDoctorByUserIdAsync(userId);
				if (doctor == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy bác sĩ theo người dùng này.");
					return NotFound(_response);
				}

				_response.Result = doctor;
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
		/// Tìm kiếm bác sĩ theo chuyên khoa
		/// </summary>
		[HttpGet("specialization/{specialization}")]
		public async Task<IActionResult> GetDoctorsBySpecialization(string specialization)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(specialization))
					return BadRequest("Chuyên khoa không được để trống.");

				var doctors = await _doctorService.GetDoctorsBySpecializationAsync(specialization);
				_response.Result = doctors;
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
		/// Kiểm tra bác sĩ có tồn tại hay không
		/// </summary>
		[HttpGet("{doctorId}/exists")]
		public async Task<IActionResult> CheckDoctorExists(int doctorId)
		{
			try
			{
				var exists = await _doctorService.DoctorExistsAsync(doctorId);
				_response.Result = new { Exists = exists, DoctorId = doctorId };
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