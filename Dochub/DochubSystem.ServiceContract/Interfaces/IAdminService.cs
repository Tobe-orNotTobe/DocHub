using DochubSystem.Data.DTOs;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface IAdminService
	{
		Task<(bool Success, string Message, string UserId, List<string> Errors)> CreateAccountAsync(RegisterAccountDTO model);
	}
}