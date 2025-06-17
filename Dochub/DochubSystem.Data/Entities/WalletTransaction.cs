using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DochubSystem.Data.Entities
{
	public class WalletTransaction
	{
		[Key]
		public int WalletTransactionId { get; set; }

		[ForeignKey("Wallet")]
		public int WalletId { get; set; }
		public Wallet Wallet { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Amount { get; set; }

		public string TransactionType { get; set; } // Deposit, Refund

		public string Description { get; set; }

		public string Status { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}