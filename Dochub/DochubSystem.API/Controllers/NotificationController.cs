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
	public class NotificationController : ControllerBase
	{
		private readonly INotificationService _notificationService;
		private readonly APIResponse _response;

		public NotificationController(INotificationService notificationService, APIResponse response)
		{
			_notificationService = notificationService;
			_response = response;
		}

		/// <summary>
		/// Lấy danh sách thông báo của user hiện tại
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> GetUserNotifications([FromQuery] GetNotificationsRequestDTO request)
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (string.IsNullOrEmpty(currentUserId))
				{
					_response.StatusCode = HttpStatusCode.Unauthorized;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("User not authenticated");
					return Unauthorized(_response);
				}

				var result = await _notificationService.GetUserNotificationsAsync(currentUserId, request);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = result;
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
		/// Lấy thông tin chi tiết một thông báo
		/// </summary>
		[HttpGet("{notificationId}")]
		public async Task<IActionResult> GetNotificationById(int notificationId)
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var notification = await _notificationService.GetNotificationByIdAsync(notificationId);

				if (notification == null || notification.UserId != currentUserId)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy thông báo");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = notification;
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
		/// Đánh dấu thông báo đã đọc
		/// </summary>
		[HttpPut("{notificationId}/mark-read")]
		public async Task<IActionResult> MarkAsRead(int notificationId)
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var result = await _notificationService.MarkAsReadAsync(notificationId, currentUserId);

				if (!result)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy thông báo hoặc đã được đọc");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = "Đã đánh dấu thông báo là đã đọc" };
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
		/// Đánh dấu nhiều thông báo đã đọc
		/// </summary>
		[HttpPut("mark-multiple-read")]
		public async Task<IActionResult> MarkMultipleAsRead([FromBody] BulkMarkReadDTO request)
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

				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var result = await _notificationService.MarkMultipleAsReadAsync(request, currentUserId);

				if (!result)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy thông báo nào để đánh dấu");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = $"Đã đánh dấu {request.NotificationIds.Count} thông báo là đã đọc" };
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
		/// Đánh dấu tất cả thông báo đã đọc
		/// </summary>
		[HttpPut("mark-all-read")]
		public async Task<IActionResult> MarkAllAsRead()
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var result = await _notificationService.MarkAllAsReadAsync(currentUserId);

				if (!result)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không có thông báo chưa đọc nào");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = "Đã đánh dấu tất cả thông báo là đã đọc" };
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
		/// Xóa thông báo
		/// </summary>
		[HttpDelete("{notificationId}")]
		public async Task<IActionResult> DeleteNotification(int notificationId)
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var result = await _notificationService.DeleteNotificationAsync(notificationId, currentUserId);

				if (!result)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy thông báo");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = "Đã xóa thông báo thành công" };
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
		/// Lấy số lượng thông báo chưa đọc
		/// </summary>
		[HttpGet("unread-count")]
		public async Task<IActionResult> GetUnreadCount()
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var count = await _notificationService.GetUnreadCountAsync(currentUserId);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { UnreadCount = count };
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
		/// Lấy thống kê thông báo của user
		/// </summary>
		[HttpGet("statistics")]
		public async Task<IActionResult> GetUserStatistics()
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var stats = await _notificationService.GetUserStatisticsAsync(currentUserId);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = stats;
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
		/// Gửi thông báo thủ công (Admin only)
		/// </summary>
		[HttpPost("send")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequestDTO request)
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

				var result = await _notificationService.SendNotificationAsync(request);

				if (!result)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không thể gửi thông báo");
					return BadRequest(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = "Đã gửi thông báo thành công" };
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
		/// Gửi thông báo hàng loạt (Admin only)
		/// </summary>
		[HttpPost("send-bulk")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> SendBulkNotification([FromBody] BulkNotificationDTO request)
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

				var result = await _notificationService.SendBulkNotificationAsync(request);

				if (!result)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không thể gửi thông báo hàng loạt");
					return BadRequest(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = $"Đã gửi thông báo cho {request.UserIds.Count} người dùng" };
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
		/// Lên lịch gửi thông báo (Admin only)
		/// </summary>
		[HttpPost("schedule")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ScheduleNotification([FromBody] SendNotificationRequestDTO request)
		{
			try
			{
				if (!ModelState.IsValid || !request.ScheduledAt.HasValue)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Scheduled time is required for scheduling notifications");
					return BadRequest(_response);
				}

				if (request.ScheduledAt.Value <= DateTime.UtcNow)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Scheduled time must be in the future");
					return BadRequest(_response);
				}

				var result = await _notificationService.ScheduleNotificationAsync(
					request.UserId,
					request.NotificationType,
					request.ScheduledAt.Value,
					request.Parameters);

				if (!result)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không thể lên lịch thông báo");
					return BadRequest(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new
				{
					Message = "Đã lên lịch thông báo thành công",
					ScheduledAt = request.ScheduledAt.Value
				};
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