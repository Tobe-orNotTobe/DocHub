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
	}
}