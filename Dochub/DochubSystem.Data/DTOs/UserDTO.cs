using System.ComponentModel.DataAnnotations;

namespace DochubSystem.Data.DTOs
{
	public class UserDTO
    {
		public string Id { get; set; }
		public string FullName { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string Address { get; set; }
		public DateTime DateOfBirth { get; set; }
		public bool IsActive { get; set; }
		public string ImageUrl { get; set; }
		public string Role { get; set; }
	}

	public class CreateUserDTO
	{
		public string FullName { get; set; }
		public string Email { get; set; }
		public string Address { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string Password { get; set; }
		public string ImageUrl { get; set; }
	}

	public class UserRegisterDTO
	{
		public string FullName { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string Address { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string Password { get; set; }
	}

	public class LoginRequestDTO
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}

	public class LoginResponseDTO
	{
		public string Token { get; set; }
		public string RefeshToken { get; set; }
	}

	public class ForgetPasswordRequestDTO
	{
		public string Email { get; set; }
	}

	public class LogoutRequestDTO
	{
		public string RefreshToken { get; set; }
	}

	public class RegisterAccountDTO
	{
		public string FullName { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string Address { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string? CertificateImageUrl { get; set; }
	}

	public class ResetPasswordDTO
	{
		[Required]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; } = string.Empty;
		public string Token { get; set; }


		[DataType(DataType.Password)]
		[Required(ErrorMessage = "Password is required")]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$",
		ErrorMessage = "Password must be at least 6 characters long, including at least one uppercase letter, one lowercase letter, one number, and one special character.")]
		public string NewPassword { get; set; } = string.Empty.ToString();
	}

	public class ChangePasswordDTO
	{
		public string OldPassword { get; set; }
		public string NewPassword { get; set; }
	}

	public class RefreshTokenRequestDTO
	{
		public string RefreshToken { get; set; }
	}
}
