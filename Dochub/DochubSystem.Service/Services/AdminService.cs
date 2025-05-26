using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DochubSystem.Service.Services
{
	public class AdminService : IAdminService
	{
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AdminService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_roleManager = roleManager;
		}

		public async Task<(bool Success, string Message, string UserId, List<string> Errors)> CreateAccountAsync(RegisterAccountDTO model)
		{
			try
			{
				// Check if user already exists
				var existingUser = await _userManager.FindByEmailAsync(model.Email);
				if (existingUser != null)
				{
					return (false, null, null, new List<string> { "Email đã được sử dụng." });
				}

				existingUser = await _userManager.FindByNameAsync(model.UserName);
				if (existingUser != null)
				{
					return (false, null, null, new List<string> { "Tên đăng nhập đã được sử dụng." });
				}

				// Create new user
				var user = new User
				{
					FullName = model.FullName,
					UserName = model.UserName,
					Email = model.Email,
					PhoneNumber = model.PhoneNumber,
					Address = model.Address,
					DateOfBirth = model.DateOfBirth,
					IsActive = true,
					CertificateImageUrl = model.CertificateImageUrl
				};

				var result = await _userManager.CreateAsync(user, model.Password);

				if (!result.Succeeded)
				{
					return (false, null, null, result.Errors.Select(e => e.Description).ToList());
				}

				// Assign role if specified
				if (!string.IsNullOrWhiteSpace(model.Role))
				{
					if (await _roleManager.RoleExistsAsync(model.Role))
					{
						await _userManager.AddToRoleAsync(user, model.Role);
					}
					else
					{
						// Rollback user creation if role doesn't exist
						await _userManager.DeleteAsync(user);
						return (false, null, null, new List<string> { $"Vai trò '{model.Role}' không tồn tại." });
					}
				}

				return (true, "Tài khoản đã được tạo thành công.", user.Id, null);
			}
			catch (Exception ex)
			{
				return (false, null, null, new List<string> { $"Lỗi hệ thống: {ex.Message}" });
			}
		}

	}
}