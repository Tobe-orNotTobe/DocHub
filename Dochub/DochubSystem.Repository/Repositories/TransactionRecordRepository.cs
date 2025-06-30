using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Repository.Repositories
{
	public class TransactionRecordRepository : Repository<TransactionRecord>, ITransactionRecordRepository
	{
		private readonly DochubDbContext _context;

		public TransactionRecordRepository(DochubDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<TransactionRecord> GetByPaymentRequestIdAsync(int paymentRequestId)
		{
			return await _context.TransactionRecords
				.Include(tr => tr.User)
				.Include(tr => tr.PaymentRequest)
				.Include(tr => tr.Plan)
				.Include(tr => tr.Subscription)
				.FirstOrDefaultAsync(tr => tr.PaymentRequestId == paymentRequestId);
		}

		public async Task<TransactionRecord> GetByTransferCodeAsync(string transferCode)
		{
			return await _context.TransactionRecords
				.Include(tr => tr.User)
				.Include(tr => tr.PaymentRequest)
				.Include(tr => tr.Plan)
				.Include(tr => tr.Subscription)
				.FirstOrDefaultAsync(tr => tr.TransferCode == transferCode);
		}

		public async Task<IEnumerable<TransactionRecord>> GetUserTransactionsAsync(string userId)
		{
			return await _context.TransactionRecords
				.Include(tr => tr.Plan)
				.Where(tr => tr.UserId == userId)
				.OrderByDescending(tr => tr.TransactionDate)
				.ToListAsync();
		}

		public async Task<IEnumerable<TransactionRecord>> GetTransactionsByDateRangeAsync(DateTime fromDate, DateTime toDate)
		{
			return await _context.TransactionRecords
				.Include(tr => tr.User)
				.Include(tr => tr.Plan)
				.Where(tr => tr.TransactionDate >= fromDate && tr.TransactionDate <= toDate)
				.OrderByDescending(tr => tr.TransactionDate)
				.ToListAsync();
		}

		public async Task<TransactionRecord> GetByIdWithDetailsAsync(int transactionId)
		{
			return await _context.TransactionRecords
				.Include(tr => tr.User)
				.Include(tr => tr.PaymentRequest)
				.Include(tr => tr.Plan)
				.Include(tr => tr.Subscription)
				.FirstOrDefaultAsync(tr => tr.TransactionId == transactionId);
		}
	}
}
