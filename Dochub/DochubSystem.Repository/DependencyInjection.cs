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
			services.AddTransient<IWalletRepository, WalletRepository>();
			services.AddTransient<IWalletTransactionRepository, WalletTransactionRepository>();
			services.AddTransient<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
			services.AddTransient<IUserSubscriptionRepository, UserSubscriptionRepository>();
			services.AddTransient<IConsultationUsageRepository, ConsultationUsageRepository>();
			services.AddTransient<IPaymentTransactionRepository, PaymentTransactionRepository>();
			services.AddTransient<INotificationRepository, NotificationRepository>();
			services.AddTransient<INotificationTemplateRepository, NotificationTemplateRepository>();
			services.AddTransient<INotificationQueueRepository, NotificationQueueRepository>();
			services.AddTransient<INotificationHistoryRepository, NotificationHistoryRepository>();


			//DI Unit of Work
			services.AddTransient<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
