using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DochubSystem.Repository.Repositories
{
	public class PaymentTransactionRepository : Repository<PaymentTransaction>, IPaymentTransactionRepository
	{
		private readonly DochubDbContext _db;

		public PaymentTransactionRepository(DochubDbContext db) : base(db)
		{
			_db = db;
		}

		public async Task<PaymentTransaction> GetByTransactionRefAsync(string transactionRef)
		{
			return await _db.PaymentTransactions
				.Include(pt => pt.User)
				.Include(pt => pt.UserSubscription)
				.FirstOrDefaultAsync(pt => pt.TransactionRef == transactionRef);
		}

		public async Task<IEnumerable<PaymentTransaction>> GetByUserIdAsync(string userId)
		{
			return await _db.PaymentTransactions
				.Include(pt => pt.UserSubscription)
				.Where(pt => pt.UserId == userId)
				.OrderByDescending(pt => pt.CreatedAt)
				.ToListAsync();
		}

		public async Task<IEnumerable<PaymentTransaction>> GetByStatusAsync(string status)
		{
			return await _db.PaymentTransactions
				.Include(pt => pt.User)
				.Include(pt => pt.UserSubscription)
				.Where(pt => pt.Status == status)
				.ToListAsync();
		}
	}
}
