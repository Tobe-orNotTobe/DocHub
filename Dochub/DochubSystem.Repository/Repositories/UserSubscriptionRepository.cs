using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Repository.Repositories
{
	public class UserSubscriptionRepository : Repository<UserSubscription>, IUserSubscriptionRepository
	{
		private readonly DochubDbContext _context;

		public UserSubscriptionRepository(DochubDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<UserSubscription> GetActiveSubscriptionByUserIdAsync(string userId)
		{
			return await GetAsync(
				s => s.UserId == userId && s.Status == "Active",
				includeProperties: "SubscriptionPlan,User");
		}

		public async Task<IEnumerable<UserSubscription>> GetExpiredSubscriptionsAsync()
		{
			return await GetAllAsync(
				s => s.Status == "Active" && s.EndDate < DateTime.UtcNow);
		}

		public async Task<IEnumerable<UserSubscription>> GetSubscriptionsForRenewalAsync()
		{
			var renewalDate = DateTime.UtcNow.AddDays(7); // 7 days before expiry
			return await GetAllAsync(
				s => s.Status == "Active" && s.EndDate <= renewalDate && s.EndDate > DateTime.UtcNow,
				includeProperties: "User,SubscriptionPlan");
		}

		public async Task<IEnumerable<UserSubscription>> GetPendingPlanChangesAsync()
		{
			return await GetAllAsync(
				s => s.PendingPlanId.HasValue && s.PlanChangeEffectiveDate <= DateTime.UtcNow,
				includeProperties: "PendingPlan,User");
		}

		public async Task<UserSubscription> GetSubscriptionWithDetailsAsync(int subscriptionId)
		{
			return await GetAsync(
				s => s.SubscriptionId == subscriptionId,
				includeProperties: "User,SubscriptionPlan,PendingPlan,Transactions");
		}
	}
}
