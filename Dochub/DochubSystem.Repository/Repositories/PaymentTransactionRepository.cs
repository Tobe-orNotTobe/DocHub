using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;

namespace DochubSystem.Repository.Repositories
{
	internal class PaymentTransactionRepository : Repository<PaymentTransaction>, IPaymentTransactionRepository
	{
		public PaymentTransactionRepository(DochubDbContext db) : base(db)
		{
		}

		public Task<IEnumerable<PaymentTransaction>> GetByStatusAsync(string status)
		{
			throw new NotImplementedException();
		}

		public Task<PaymentTransaction> GetByTransactionRefAsync(string transactionRef)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<PaymentTransaction>> GetByUserIdAsync(string userId)
		{
			throw new NotImplementedException();
		}
	}
}
