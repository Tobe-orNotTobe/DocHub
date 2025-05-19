using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Repository
{
    public static class DatabaseConfiguration
    {
        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<DochubDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

           // // Register Identity with EF
           //services.AddIdentity<User, IdentityRole>()
           //     .AddEntityFrameworkStores<DochubDbContext>()
           //     .AddDefaultTokenProviders();

            return services;
        }
    }
}
