using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;

namespace DochubSystem.Repository.Repositories
{
	public class SubscriptionPlanRepository : Repository<SubscriptionPlan>, ISubscriptionPlanRepository
	{
		public SubscriptionPlanRepository(DochubDbContext context) : base(context)
		{
		}

		public async Task<IEnumerable<SubscriptionPlan>> GetActivePlansAsync()
		{
			return await GetAllAsync(p => p.IsActive);
		}

		public async Task<SubscriptionPlan> GetPlanByNameAsync(string name)
		{
			return await GetAsync(p => p.Name.ToLower() == name.ToLower() && p.IsActive);
		}
	}
}
