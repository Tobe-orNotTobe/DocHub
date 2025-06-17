using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Repository.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DochubDbContext _context;

        public IUserRepository Users { get; }
		public IDoctorRepository Doctors { get; }
		public IAppointmentRepository Appointments { get; }
		public IAppointmentTransactionRepository AppointmentTransactions { get; }
		public ISubscriptionPlanRepository SubscriptionPlans { get; }
		public IUserSubscriptionRepository UserSubscriptions { get; }
		public IConsultationUsageRepository ConsultationUsages { get; }

		public UnitOfWork(DochubDbContext context,
					   IUserRepository userRepository,
						IDoctorRepository doctorRepository,
					   IAppointmentRepository appointmentRepository,
					   IAppointmentTransactionRepository appointmentTransactionRepository)
		{
			_context = context;
			Users = userRepository;
			Doctors = doctorRepository;
			Appointments = appointmentRepository;
			AppointmentTransactions = appointmentTransactionRepository;
		}


		public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
