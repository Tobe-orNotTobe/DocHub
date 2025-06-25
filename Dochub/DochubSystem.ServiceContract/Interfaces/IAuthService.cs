using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface IAuthService
	{
		Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO);
		Task<User> RegisterAsync(UserRegisterDTO dto);
		Task<bool> ConfirmEmailAsync(string email, string token);
		Task<LoginResponseDTO> RefreshTokenAsync(string token);
		Task LogoutAsync(string refreshToken);
		Task<bool> ForgetPasswordAsync(string email);
		Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword);
        Task<(bool Success, string Message)> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
        string GenerateJwtToken(User user);
	}
}
