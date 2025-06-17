using DochubSystem.Data.DTOs;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface IPaymentService
	{
		// VNPay Integration
		Task<PaymentUrlResponseDTO> CreateVNPayPaymentUrlAsync(CreatePaymentRequestDTO request);
		Task<PaymentVerificationDTO> VerifyVNPayPaymentAsync(Dictionary<string, string> vnpayData);

		// MoMo Integration
		Task<PaymentUrlResponseDTO> CreateMoMoPaymentUrlAsync(CreatePaymentRequestDTO request);

	
	}
}
