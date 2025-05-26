using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;

namespace DochubSystem.Repository.Repositories
{
	public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
	{
		public AppointmentRepository(DochubDbContext context) : base(context)
		{
		}

	}
}
