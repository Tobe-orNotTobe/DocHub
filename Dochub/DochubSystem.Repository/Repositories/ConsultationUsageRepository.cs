using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DochubSystem.Repository.Repositories
{
	public class ConsultationUsageRepository : Repository<ConsultationUsage>, IConsultationUsageRepository
	{
		private readonly DochubDbContext _context;

		public ConsultationUsageRepository(DochubDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<IEnumerable<ConsultationUsage>> GetUsageByUserIdAsync(string userId)
		{
			return await GetAllAsync(
				u => u.UserId == userId,
				includeProperties: "Appointment.Doctor.User,UserSubscription");
		}

		public async Task<IEnumerable<ConsultationUsage>> GetUsageBySubscriptionIdAsync(int subscriptionId)
		{
			return await GetAllAsync(
				u => u.SubscriptionId == subscriptionId,
				includeProperties: "Appointment.Doctor.User");
		}

		public async Task<int> GetUsageCountForCurrentPeriodAsync(string userId)
		{
			var activeSubscription = await _context.Set<UserSubscription>()
				.FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "Active");

			if (activeSubscription == null)
				return 0;

			return await _context.Set<ConsultationUsage>()
				.CountAsync(u => u.UserId == userId &&
							   u.SubscriptionId == activeSubscription.SubscriptionId &&
							   u.UsageDate >= activeSubscription.StartDate);
		}
	}
}
