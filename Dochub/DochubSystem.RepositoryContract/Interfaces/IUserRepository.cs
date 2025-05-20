using DochubSystem.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.RepositoryContract.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(string userId);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IdentityResult> CreateUserAsync(User user, string password);
        Task<IdentityResult> UpdateUserAsync(User user);
        Task<IdentityResult> DeleteUserAsync(User user);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string userId);
    }
}
