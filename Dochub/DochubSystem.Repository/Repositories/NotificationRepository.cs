using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Repository.Repositories
{
	public class NotificationRepository : Repository<Notification>, INotificationRepository
	{
		private readonly DochubDbContext _context;

		public NotificationRepository(DochubDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, GetNotificationsRequestDTO request)
		{
			var query = _context.Notifications
				.Where(n => n.UserId == userId)
				.Include(n => n.Appointment)
				.Include(n => n.Doctor)
				.ThenInclude(d => d.User)
				.AsQueryable();

			// Apply filters
			if (!string.IsNullOrEmpty(request.Status) && request.Status != "all")
			{
				query = query.Where(n => n.Status == request.Status);
			}

			if (!string.IsNullOrEmpty(request.Type))
			{
				query = query.Where(n => n.Type == request.Type);
			}

			if (request.FromDate.HasValue)
			{
				query = query.Where(n => n.CreatedAt >= request.FromDate.Value);
			}

			if (request.ToDate.HasValue)
			{
				query = query.Where(n => n.CreatedAt <= request.ToDate.Value);
			}

			// Apply pagination and ordering
			query = query.OrderByDescending(n => n.CreatedAt)
						 .Skip((request.Page - 1) * request.PageSize)
						 .Take(request.PageSize);

			return await query.ToListAsync();
		}

		public async Task<int> GetUnreadCountAsync(string userId)
		{
			return await _context.Notifications
				.CountAsync(n => n.UserId == userId && n.Status == "unread");
		}

		public async Task<NotificationStatisticsDTO> GetUserNotificationStatisticsAsync(string userId)
		{
			var today = DateTime.UtcNow.Date;
			var notifications = await _context.Notifications
				.Where(n => n.UserId == userId)
				.ToListAsync();

			var stats = new NotificationStatisticsDTO
			{
				TotalNotifications = notifications.Count,
				UnreadNotifications = notifications.Count(n => n.Status == "unread"),
				ReadNotifications = notifications.Count(n => n.Status == "read"),
				HighPriorityNotifications = notifications.Count(n => n.Priority == "high" || n.Priority == "urgent"),
				TodayNotifications = notifications.Count(n => n.CreatedAt.Date == today),
				NotificationsByType = notifications
					.GroupBy(n => n.Type)
					.ToDictionary(g => g.Key, g => g.Count())
			};

			return stats;
		}

		public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
		{
			var notification = await _context.Notifications
				.FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

			if (notification == null || notification.Status == "read")
				return false;

			notification.Status = "read";
			notification.ReadAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> MarkMultipleAsReadAsync(List<int> notificationIds, string userId)
		{
			var notifications = await _context.Notifications
				.Where(n => notificationIds.Contains(n.NotificationId) && n.UserId == userId && n.Status == "unread")
				.ToListAsync();

			if (!notifications.Any())
				return false;

			foreach (var notification in notifications)
			{
				notification.Status = "read";
				notification.ReadAt = DateTime.UtcNow;
			}

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> MarkAllAsReadAsync(string userId)
		{
			var unreadNotifications = await _context.Notifications
				.Where(n => n.UserId == userId && n.Status == "unread")
				.ToListAsync();

			if (!unreadNotifications.Any())
				return false;

			foreach (var notification in unreadNotifications)
			{
				notification.Status = "read";
				notification.ReadAt = DateTime.UtcNow;
			}

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(string userId, string type, int limit = 10)
		{
			return await _context.Notifications
				.Where(n => n.UserId == userId && n.Type == type)
				.OrderByDescending(n => n.CreatedAt)
				.Take(limit)
				.ToListAsync();
		}

		public async Task<bool> DeleteOldNotificationsAsync(DateTime cutoffDate)
		{
			var oldNotifications = await _context.Notifications
				.Where(n => n.CreatedAt < cutoffDate)
				.ToListAsync();

			if (!oldNotifications.Any())
				return false;

			_context.Notifications.RemoveRange(oldNotifications);
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
