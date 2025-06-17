using DochubSystem.Repository.Repositories;
using DochubSystem.RepositoryContract.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DochubSystem.Repository
{
	public static class DependencyInjection
    {
        public static IServiceCollection AddRepository(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureDatabase(configuration);

            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

            services.AddTransient<IUserRepository, UserRepository>();
			services.AddTransient<IDoctorRepository, DoctorRepository>();
			services.AddTransient<IAppointmentRepository, AppointmentRepository>();
			services.AddTransient<IAppointmentTransactionRepository, AppointmentTransactionRepository>();
			services.AddTransient<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
			services.AddTransient<IUserSubscriptionRepository, UserSubscriptionRepository>();
			services.AddTransient<IConsultationUsageRepository, ConsultationUsageRepository>();

			//DI Unit of Work
			services.AddTransient<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
