using DochubSystem.Data.DTOs;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface IAdminService
	{
		Task<(bool Success, string Message, string UserId, List<string> Errors)> CreateAccountAsync(RegisterAccountDTO model);
		Task<IEnumerable<object>> GetAllUsersWithRolesAsync();
		Task<object> GetUserByIdWithRolesAsync(string userId);
		Task<(bool Success, string Message, List<string> Errors)> DeleteUserAsync(string userId, string currentUserId);
		Task<(bool Success, string Message, List<string> Errors)> UpdateUserAsync(UserDTO model);
		Task<(bool Success, string Message, List<string> Errors)> ActivateUserAsync(string userId);
		Task<(bool Success, string Message, List<string> Errors)> DeactivateUserAsync(string userId);
		Task<IEnumerable<object>> GetAllRolesAsync();
		Task<(bool Success, string Message, List<string> Errors)> CreateRoleAsync(string roleName);
	}
}