using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Repository.Repositories
{
	public class UserRepository : Repository<User>, IUserRepository
	{
		private readonly UserManager<User> _userManager;
		private readonly DochubDbContext _context;

		public UserRepository(UserManager<User> userManager, DochubDbContext context) : base(context)
		{
			_userManager = userManager;
			_context = context;
		}

		public async Task<User> GetUserByIdAsync(string userId)
		{
			return await _userManager.Users
									 .FirstOrDefaultAsync(u => u.Id == userId);
		}

		public async Task<IEnumerable<User>> GetAllUsersAsync()			
		{
			return await _userManager.Users.ToListAsync();
		}

		public async Task<IdentityResult> CreateUserAsync(User user, string password)
		{
			return await _userManager.CreateAsync(user, password);
		}

		public async Task<IdentityResult> UpdateUserAsync(User user)
		{
			return await _userManager.UpdateAsync(user);
		}

		public async Task<IdentityResult> DeleteUserAsync(User user)
		{
			return await _userManager.DeleteAsync(user);
		}

		public async Task<User> GetUserByEmailAsync(string email)
		{
			return await _userManager.FindByEmailAsync(email);
		}

		public async Task<bool> UserExistsAsync(string userId)
		{
			return await _userManager.Users.AnyAsync(u => u.Id == userId);
		}
	}
}

