using DochubSystem.Common.Helper;
using DochubSystem.RepositoryContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DochubSystem.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
	public class NotificationManagementController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly APIResponse _response;

		public NotificationManagementController(IUnitOfWork unitOfWork, APIResponse response)
		{
			_unitOfWork = unitOfWork;
			_response = response;
		}

		/// <summary>
		/// Lấy danh sách notification queue
		/// </summary>
		[HttpGet("queue")]
		public async Task<IActionResult> GetNotificationQueue([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
		{
			try
			{
				var query = await _unitOfWork.NotificationQueues.GetAllAsync(
					filter: !string.IsNullOrEmpty(status) ? nq => nq.Status == status : null,
					includeProperties: "User,NotificationTemplate"
				);

				var totalCount = query.Count();
				var queueItems = query
					.OrderByDescending(nq => nq.CreatedAt)
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.Select(nq => new
					{
						nq.QueueId,
						nq.TemplateId,
						TemplateName = nq.NotificationTemplate.Name,
						nq.UserId,
						UserName = nq.User.FullName,
						UserEmail = nq.User.Email,
						nq.Subject,
						nq.Status,
						nq.Priority,
						nq.NotificationType,
						nq.ScheduledAt,
						nq.SentAt,
						nq.CreatedAt,
						nq.RetryCount,
						nq.ErrorMessage
					})
					.ToList();

				var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new
				{
					QueueItems = queueItems,
					TotalCount = totalCount,
					Page = page,
					PageSize = pageSize,
					TotalPages = totalPages,
					HasNextPage = page < totalPages,
					HasPreviousPage = page > 1
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

		/// <summary>
		/// Lấy thống kê queue
		/// </summary>
		[HttpGet("queue/statistics")]
		public async Task<IActionResult> GetQueueStatistics()
		{
			try
			{
				var pendingCount = await _unitOfWork.NotificationQueues.GetQueueCountByStatusAsync("pending");
				var sentCount = await _unitOfWork.NotificationQueues.GetQueueCountByStatusAsync("sent");
				var failedCount = await _unitOfWork.NotificationQueues.GetQueueCountByStatusAsync("failed");
				var cancelledCount = await _unitOfWork.NotificationQueues.GetQueueCountByStatusAsync("cancelled");

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new
				{
					PendingCount = pendingCount,
					SentCount = sentCount,
					FailedCount = failedCount,
					CancelledCount = cancelledCount,
					TotalCount = pendingCount + sentCount + failedCount + cancelledCount
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

		/// <summary>
		/// Retry failed notification
		/// </summary>
		[HttpPut("queue/{queueId}/retry")]
		public async Task<IActionResult> RetryNotification(int queueId)
		{
			try
			{
				var queueItem = await _unitOfWork.NotificationQueues.GetAsync(nq => nq.QueueId == queueId);
				if (queueItem == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy notification trong queue");
					return NotFound(_response);
				}

				if (queueItem.Status != "failed")
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Chỉ có thể retry notification đã failed");
					return BadRequest(_response);
				}

				// Reset status to pending for retry
				await _unitOfWork.NotificationQueues.UpdateStatusAsync(queueId, "pending", null);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = "Đã đặt lại notification để retry" };
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
		/// Cancel pending notification
		/// </summary>
		[HttpPut("queue/{queueId}/cancel")]
		public async Task<IActionResult> CancelNotification(int queueId)
		{
			try
			{
				var queueItem = await _unitOfWork.NotificationQueues.GetAsync(nq => nq.QueueId == queueId);
				if (queueItem == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Không tìm thấy notification trong queue");
					return NotFound(_response);
				}

				if (queueItem.Status != "pending")
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Chỉ có thể cancel notification đang pending");
					return BadRequest(_response);
				}

				await _unitOfWork.NotificationQueues.UpdateStatusAsync(queueId, "cancelled", "Cancelled by admin");

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = "Đã hủy notification thành công" };
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
		/// Lấy notification history
		/// </summary>
		[HttpGet("history")]
		public async Task<IActionResult> GetNotificationHistory([FromQuery] string? userId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
		{
			try
			{
				IEnumerable<DochubSystem.Data.Entities.NotificationHistory> histories;

				if (!string.IsNullOrEmpty(userId))
				{
					histories = await _unitOfWork.NotificationHistories.GetUserHistoryAsync(userId, page, pageSize);
				}
				else
				{
					var allHistories = await _unitOfWork.NotificationHistories.GetAllAsync(includeProperties: "User,NotificationTemplate");
					histories = allHistories
						.OrderByDescending(h => h.SentAt)
						.Skip((page - 1) * pageSize)
						.Take(pageSize);
				}

				var historyDTOs = histories.Select(h => new
				{
					h.HistoryId,
					h.UserId,
					UserName = h.User?.FullName,
					UserEmail = h.User?.Email,
					h.TemplateId,
					TemplateName = h.NotificationTemplate?.Name,
					h.Subject,
					h.NotificationType,
					h.DeliveryMethod,
					h.Status,
					h.SentAt,
					h.ReadAt,
					h.ErrorMessage,
					h.AppointmentId,
					h.DoctorId
				}).ToList();

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new
				{
					Histories = historyDTOs,
					Page = page,
					PageSize = pageSize
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

		/// <summary>
		/// Lấy thống kê notification theo type
		/// </summary>
		[HttpGet("statistics/by-type")]
		public async Task<IActionResult> GetStatisticsByType([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
		{
			try
			{
				var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
				var to = toDate ?? DateTime.UtcNow;

				var stats = await _unitOfWork.NotificationHistories.GetNotificationStatsByTypeAsync(from, to);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new
				{
					FromDate = from,
					ToDate = to,
					StatsByType = stats,
					TotalNotifications = stats.Values.Sum()
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

		/// <summary>
		/// Cleanup old notifications
		/// </summary>
		[HttpDelete("cleanup")]
		public async Task<IActionResult> CleanupOldNotifications([FromQuery] int daysOld = 90)
		{
			try
			{
				var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);

				// Cleanup old processed queue items
				var queueCleanup = await _unitOfWork.NotificationQueues.ClearOldProcessedNotificationsAsync(cutoffDate);

				// Cleanup old notification history
				var historyCleanup = await _unitOfWork.NotificationHistories.CleanupOldHistoryAsync(cutoffDate);

				// Cleanup old notifications
				var notificationCleanup = await _unitOfWork.Notifications.DeleteOldNotificationsAsync(cutoffDate);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new
				{
					Message = $"Đã cleanup notifications cũ hơn {daysOld} ngày",
					CutoffDate = cutoffDate,
					QueueItemsCleanedUp = queueCleanup,
					HistoryItemsCleanedUp = historyCleanup,
					NotificationsCleanedUp = notificationCleanup
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

		/// <summary>
		/// Get system notification overview
		/// </summary>
		[HttpGet("overview")]
		public async Task<IActionResult> GetSystemOverview()
		{
			try
			{
				var today = DateTime.UtcNow.Date;
				var thisWeek = DateTime.UtcNow.AddDays(-7);
				var thisMonth = DateTime.UtcNow.AddDays(-30);

				// Queue statistics
				var pendingCount = await _unitOfWork.NotificationQueues.GetQueueCountByStatusAsync("pending");
				var failedCount = await _unitOfWork.NotificationQueues.GetQueueCountByStatusAsync("failed");

				// Today's notifications
				var todayNotifications = await _unitOfWork.NotificationHistories.GetAllAsync(
					h => h.SentAt >= today);

				// Weekly statistics
				var weeklyNotifications = await _unitOfWork.NotificationHistories.GetAllAsync(
					h => h.SentAt >= thisWeek);

				// Monthly statistics
				var monthlyNotifications = await _unitOfWork.NotificationHistories.GetAllAsync(
					h => h.SentAt >= thisMonth);

				// Template count
				var activeTemplates = await _unitOfWork.NotificationTemplates.GetActiveTemplatesAsync();

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new
				{
					Queue = new
					{
						PendingCount = pendingCount,
						FailedCount = failedCount
					},
					Statistics = new
					{
						TodayCount = todayNotifications.Count(),
						WeeklyCount = weeklyNotifications.Count(),
						MonthlyCount = monthlyNotifications.Count(),
						ActiveTemplatesCount = activeTemplates.Count()
					},
					RecentActivity = todayNotifications
						.OrderByDescending(h => h.SentAt)
						.Take(10)
						.Select(h => new
						{
							h.NotificationType,
							h.Status,
							h.SentAt,
							UserEmail = h.User?.Email
						})
						.ToList()
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