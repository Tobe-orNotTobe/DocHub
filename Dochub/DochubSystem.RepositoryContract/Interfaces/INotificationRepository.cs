using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;

namespace DochubSystem.RepositoryContract.Interfaces
{
	public interface INotificationRepository : IRepository<Notification>
	{
		Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, GetNotificationsRequestDTO request);
		Task<int> GetUnreadCountAsync(string userId);
		Task<NotificationStatisticsDTO> GetUserNotificationStatisticsAsync(string userId);
		Task<bool> MarkAsReadAsync(int notificationId, string userId);
		Task<bool> MarkMultipleAsReadAsync(List<int> notificationIds, string userId);
		Task<bool> MarkAllAsReadAsync(string userId);
		Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(string userId, string type, int limit = 10);
		Task<bool> DeleteOldNotificationsAsync(DateTime cutoffDate);
	}
}
