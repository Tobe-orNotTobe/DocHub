using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DochubSystem.Repository.Repositories
{
	public class DoctorRepository : Repository<Doctor>, IDoctorRepository
	{
		public DoctorRepository(DochubDbContext context) : base(context)
		{
		}
	}
}
