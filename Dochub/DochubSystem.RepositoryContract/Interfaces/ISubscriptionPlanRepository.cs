using DochubSystem.Data.Entities;

namespace DochubSystem.RepositoryContract.Interfaces
{
	public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
	{
		Task<IEnumerable<SubscriptionPlan>> GetActivePlansAsync();
		Task<SubscriptionPlan> GetPlanByNameAsync(string name);
	}
}
