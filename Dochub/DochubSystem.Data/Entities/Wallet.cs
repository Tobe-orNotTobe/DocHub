using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Data.Entities
{
	public class Wallet
	{
		[Key]
		public int WalletId { get; set; }

		[ForeignKey("User")]
		public string UserId { get; set; }
		public User User { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Balance { get; set; } = 0;

		[Column(TypeName = "decimal(18,2)")]
		public decimal TotalRefunded { get; set; } = 0;

		public bool IsAdminWallet { get; set; } = false;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }

		public virtual ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
	}
}
