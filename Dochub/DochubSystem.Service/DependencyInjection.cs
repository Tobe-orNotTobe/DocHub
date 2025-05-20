using DochubSystem.Common.Helper;
using DochubSystem.Repository;
using DochubSystem.Service.Services;
using DochubSystem.ServiceContract.Interfaces;
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

            return services;
        }
    }
}
