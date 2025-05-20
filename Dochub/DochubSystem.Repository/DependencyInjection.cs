using DochubSystem.Repository.Repositories;
using DochubSystem.Repository.Repositories.DochubSystem.Repository.Repositories;
using DochubSystem.RepositoryContract.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Repository
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepository(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureDatabase(configuration);

            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

            services.AddTransient<IUserRepository, UserRepository>();

            //DI Unit of Work
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
