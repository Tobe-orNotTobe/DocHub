using DochubSystem.Common.Helper;
using DochubSystem.Repository;
using DochubSystem.Service.BackgroundServices;
using DochubSystem.Service.Services;
using DochubSystem.ServiceContract.Interfaces;
using MailKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DochubSystem.Service
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký các Repository
            services.AddRepository(configuration);

            // Đăng ký APIResponse (Transient)
            services.AddScoped<APIResponse>();

            // Đăng ký UserService (Transient)
            services.AddTransient<IUserService, UserService>();
			services.AddTransient<IAuthService, AuthService>();
			services.AddTransient<IAdminService, AdminService>();
			services.AddTransient<IEmailService, EmailService>();
			services.AddTransient<IDoctorService, DoctorService>();
			services.AddTransient<IAppointmentService, AppointmentService>();
			services.AddTransient<IPaymentService, PaymentService>();
			services.AddTransient<ISubscriptionService, SubscriptionService>();
			services.AddScoped<INotificationService, NotificationService>();
			services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
			services.AddHostedService<StartupInitializationService>();
			services.AddHostedService<NotificationBackgroundService>();
			services.AddHostedService<HighPriorityNotificationService>();
			services.AddHostedService<AppointmentReminderService>();

			return services;
        }
    }
}
