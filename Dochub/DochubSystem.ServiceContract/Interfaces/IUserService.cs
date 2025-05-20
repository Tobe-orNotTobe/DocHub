using DochubSystem.Data.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.ServiceContract.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO> GetUserByIdAsync(string userId);
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<IdentityResult> CreateUserAsync(CreateUserDTO createUserDTO);
        Task<IdentityResult> UpdateUserAsync(string userId, UpdateUserDTO updateUserDTO);
        Task<IdentityResult> DeleteUserAsync(string userId);
    }
}
