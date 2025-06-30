using DochubSystem.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.RepositoryContract.Interfaces
{
	public interface ITransactionRecordRepository : IRepository<TransactionRecord>
	{
		Task<TransactionRecord> GetByPaymentRequestIdAsync(int paymentRequestId);
		Task<TransactionRecord> GetByTransferCodeAsync(string transferCode);
		Task<IEnumerable<TransactionRecord>> GetUserTransactionsAsync(string userId);
		Task<IEnumerable<TransactionRecord>> GetTransactionsByDateRangeAsync(DateTime fromDate, DateTime toDate);
		Task<TransactionRecord> GetByIdWithDetailsAsync(int transactionId);
	}
}
