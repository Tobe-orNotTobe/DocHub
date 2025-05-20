using AutoMapper;
using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using DochubSystem.RepositoryContract.Interfaces;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, UserManager<User> userManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<UserDTO> GetUserByIdAsync(string userId)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllUsersAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<IdentityResult> CreateUserAsync(CreateUserDTO createUserDTO)
        {
            var user = _mapper.Map<User>(createUserDTO);
            var result = await _userManager.CreateAsync(user, createUserDTO.Password);
            if (result.Succeeded)
            {
                await _unitOfWork.CompleteAsync();
            }
            return result;
        }

        public async Task<IdentityResult> UpdateUserAsync(string userId, UpdateUserDTO updateUserDTO)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null) return IdentityResult.Failed();

            user.FullName = updateUserDTO.FullName;
            user.Address = updateUserDTO.Address;
            user.DateOfBirth = updateUserDTO.DateOfBirth;
            user.ImageUrl = updateUserDTO.ImageUrl;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _unitOfWork.CompleteAsync();
            }
            return result;
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null) return IdentityResult.Failed();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _unitOfWork.CompleteAsync();
            }
            return result;
        }
    }
}
