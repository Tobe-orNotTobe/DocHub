using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;

namespace DochubSystem.Repository.Repositories
{
	public class AppointmentTransactionRepository : Repository<AppointmentTransaction>, IAppointmentTransactionRepository
	{
		public AppointmentTransactionRepository(DochubDbContext db) : base(db)
		{
		}
	}
}
