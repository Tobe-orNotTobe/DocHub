using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;

namespace DochubSystem.Repository.Repositories
{
	public class WalletTransactionRepository : Repository<WalletTransaction>, IWalletTransactionRepository
	{
		public WalletTransactionRepository(DochubDbContext db) : base(db)
		{
		}
	}
}
