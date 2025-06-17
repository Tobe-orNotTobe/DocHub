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
		private readonly IEmailService _emailService;

		public AdminService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IEmailService emailService)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_emailService = emailService;
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
					CertificateImageUrl = model.CertificateImageUrl,
					EmailConfirmed = true
				};

				string password = GeneratePassword(10);

				var result = await _userManager.CreateAsync(user, password);

				if (!result.Succeeded)
				{
					return (false, null, null, result.Errors.Select(e => e.Description).ToList());
				}

				await _userManager.AddToRoleAsync(user, "Doctor");

				try
				{
					_emailService.SendWelcomeEmail(model.Email, model.FullName, model.UserName, password);
				}
				catch (Exception emailEx)
				{
					
					Console.WriteLine($"Failed to send welcome email: {emailEx.Message}");
				}

				return (true, "Tài khoản đã được tạo thành công.", user.Id, null);
			}
			catch (Exception ex)
			{
				return (false, null, null, new List<string> { $"Lỗi hệ thống: {ex.Message}" });
			}
		}

		private static string GeneratePassword(int length)
		{
			const string lower = "abcdefghijklmnopqrstuvwxyz";
			const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			const string digits = "0123456789";
			const string special = "!@#$%^&*";
			string all = lower + upper + digits + special;

			var rand = new Random();
			var password = new char[length];

			password[0] = lower[rand.Next(lower.Length)];
			password[1] = upper[rand.Next(upper.Length)];
			password[2] = digits[rand.Next(digits.Length)];
			password[3] = special[rand.Next(special.Length)];

			for (int i = 4; i < length; i++)
				password[i] = all[rand.Next(all.Length)];

			return new string(password.OrderBy(_ => rand.Next()).ToArray());
		}

		public async Task<IEnumerable<object>> GetAllUsersWithRolesAsync()
		{
			var users = await _userManager.Users.ToListAsync();
			var userWithRoles = new List<object>();

			foreach (var user in users)
			{
				var roles = await _userManager.GetRolesAsync(user);

				userWithRoles.Add(new
				{
					user.Id,
					user.UserName,
					user.FullName,
					user.Email,
					user.Address,
					user.DateOfBirth,
					user.IsActive,
					user.PhoneNumber,
					user.ImageUrl,
					user.CertificateImageUrl,
					Roles = roles
				});
			}

			return userWithRoles;
		}

		public async Task<object> GetUserByIdWithRolesAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return null;
			}

			var roles = await _userManager.GetRolesAsync(user);

			return new
			{
				user.Id,
				user.UserName,
				user.FullName,
				user.Email,
				user.Address,
				user.DateOfBirth,
				user.IsActive,
				user.PhoneNumber,
				user.ImageUrl,
				user.CertificateImageUrl,
				Roles = roles
			};
		}

		public async Task<(bool Success, string Message, List<string> Errors)> DeleteUserAsync(string userId, string currentUserId)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId);
				if (user == null)
				{
					return (false, null, new List<string> { "Không tìm thấy người dùng" });
				}

				// Check if the user is trying to delete themselves
				if (currentUserId == userId)
				{
					return (false, null, new List<string> { "Không thể xóa tài khoản của chính mình" });
				}

				var result = await _userManager.DeleteAsync(user);
				if (!result.Succeeded)
				{
					return (false, null, result.Errors.Select(e => e.Description).ToList());
				}

				return (true, "Người dùng đã được xóa thành công", null);
			}
			catch (Exception ex)
			{
				return (false, null, new List<string> { $"Lỗi khi xóa người dùng: {ex.Message}" });
			}
		}

		public async Task<(bool Success, string Message, List<string> Errors)> UpdateUserAsync(UserDTO model)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(model.Id);
				if (user == null)
				{
					return (false, null, new List<string> { "Không tìm thấy người dùng" });
				}

				// Update user properties
				user.UserName = model.UserName ?? user.UserName;
				user.FullName = model.FullName ?? user.FullName;
				user.Email = model.Email ?? user.Email;
				user.PhoneNumber = model.PhoneNumber ?? user.PhoneNumber;
				user.Address = model.Address ?? user.Address;
				user.DateOfBirth = model.DateOfBirth;
				user.ImageUrl = model.ImageUrl ?? user.ImageUrl;

				// Update role if provided
				if (!string.IsNullOrWhiteSpace(model.Role))
				{
					var existingRoles = await _userManager.GetRolesAsync(user);

					// Remove existing roles
					if (existingRoles.Any())
					{
						var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);
						if (!removeResult.Succeeded)
						{
							return (false, null, removeResult.Errors.Select(e => e.Description).ToList());
						}
					}

					// Check if new role exists
					if (!await _roleManager.RoleExistsAsync(model.Role))
					{
						return (false, null, new List<string> { $"Vai trò '{model.Role}' không tồn tại" });
					}

					// Add new role
					var addResult = await _userManager.AddToRoleAsync(user, model.Role);
					if (!addResult.Succeeded)
					{
						return (false, null, addResult.Errors.Select(e => e.Description).ToList());
					}
				}

				var result = await _userManager.UpdateAsync(user);
				if (!result.Succeeded)
				{
					return (false, null, result.Errors.Select(e => e.Description).ToList());
				}

				return (true, "Người dùng đã được cập nhật thành công", null);
			}
			catch (Exception ex)
			{
				return (false, null, new List<string> { $"Lỗi khi cập nhật người dùng: {ex.Message}" });
			}
		}

		public async Task<(bool Success, string Message, List<string> Errors)> ActivateUserAsync(string userId)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId);
				if (user == null)
				{
					return (false, null, new List<string> { "Không tìm thấy người dùng" });
				}

				if (user.IsActive)
				{
					return (false, null, new List<string> { "Người dùng đã được kích hoạt" });
				}

				user.IsActive = true;
				var result = await _userManager.UpdateAsync(user);

				if (!result.Succeeded)
				{
					return (false, null, result.Errors.Select(e => e.Description).ToList());
				}

				return (true, "Người dùng đã được kích hoạt thành công", null);
			}
			catch (Exception ex)
			{
				return (false, null, new List<string> { $"Lỗi khi kích hoạt người dùng: {ex.Message}" });
			}
		}

		public async Task<(bool Success, string Message, List<string> Errors)> DeactivateUserAsync(string userId)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId);
				if (user == null)
				{
					return (false, null, new List<string> { "Không tìm thấy người dùng" });
				}

				if (!user.IsActive)
				{
					return (false, null, new List<string> { "Người dùng đã bị vô hiệu hóa" });
				}

				user.IsActive = false;
				var result = await _userManager.UpdateAsync(user);

				if (!result.Succeeded)
				{
					return (false, null, result.Errors.Select(e => e.Description).ToList());
				}

				return (true, "Người dùng đã bị vô hiệu hóa thành công", null);
			}
			catch (Exception ex)
			{
				return (false, null, new List<string> { $"Lỗi khi vô hiệu hóa người dùng: {ex.Message}" });
			}
		}


		public async Task<IEnumerable<object>> GetAllRolesAsync()
		{
			return await _roleManager.Roles
				.Select(role => new
				{
					role.Name,
					role.Id
				})
				.ToListAsync();
		}

		public async Task<(bool Success, string Message, List<string> Errors)> CreateRoleAsync(string roleName)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(roleName))
				{
					return (false, null, new List<string> { "Tên vai trò không được để trống" });
				}

				if (await _roleManager.RoleExistsAsync(roleName))
				{
					return (false, null, new List<string> { $"Vai trò '{roleName}' đã tồn tại" });
				}

				var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
				if (!result.Succeeded)
				{
					return (false, null, result.Errors.Select(e => e.Description).ToList());
				}

				return (true, $"Vai trò '{roleName}' đã được tạo thành công", null);
			}
			catch (Exception ex)
			{
				return (false, null, new List<string> { $"Lỗi khi tạo vai trò: {ex.Message}" });
			}
		}
	}
}