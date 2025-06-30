using DochubSystem.Data.DTOs;
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
	public class PaymentRequestRepository : Repository<PaymentRequest>, IPaymentRequestRepository
	{
		private readonly DochubDbContext _context;

		public PaymentRequestRepository(DochubDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<PaymentRequest> GetByTransferCodeAsync(string transferCode)
		{
			return await _context.PaymentRequests
				.Include(pr => pr.User)
				.Include(pr => pr.Plan)
				.FirstOrDefaultAsync(pr => pr.TransferCode == transferCode);
		}

		public async Task<PaymentRequest> GetByIdWithDetailsAsync(int paymentRequestId)
		{
			return await _context.PaymentRequests
				.Include(pr => pr.User)
				.Include(pr => pr.Plan)
				.FirstOrDefaultAsync(pr => pr.PaymentRequestId == paymentRequestId);
		}

		public async Task<IEnumerable<PaymentRequest>> GetPendingRequestsAsync()
		{
			return await _context.PaymentRequests
				.Include(pr => pr.User)
				.Include(pr => pr.Plan)
				.Where(pr => pr.Status == "Pending" && pr.ExpiresAt > DateTime.UtcNow)
				.OrderByDescending(pr => pr.CreatedAt)
				.ToListAsync();
		}

		public async Task<IEnumerable<PaymentRequest>> GetExpiredRequestsAsync()
		{
			return await _context.PaymentRequests
				.Where(pr => pr.Status == "Pending" && pr.ExpiresAt <= DateTime.UtcNow)
				.ToListAsync();
		}

		public async Task<IEnumerable<PaymentRequest>> GetUserPaymentRequestsAsync(string userId)
		{
			return await _context.PaymentRequests
				.Include(pr => pr.Plan)
				.Include(pr => pr.ConfirmedByAdmin)
				.Where(pr => pr.UserId == userId)
				.OrderByDescending(pr => pr.CreatedAt)
				.ToListAsync();
		}

		public async Task<IEnumerable<PaymentRequest>> SearchPaymentRequestsAsync(PaymentRequestSearchDTO searchDto)
		{
			var query = _context.PaymentRequests
				.Include(pr => pr.User)
				.Include(pr => pr.Plan)
				.AsQueryable();

			if (!string.IsNullOrEmpty(searchDto.TransferCode))
			{
				query = query.Where(pr => pr.TransferCode.Contains(searchDto.TransferCode));
			}

			if (!string.IsNullOrEmpty(searchDto.Status))
			{
				query = query.Where(pr => pr.Status == searchDto.Status);
			}

			if (searchDto.FromDate.HasValue)
			{
				query = query.Where(pr => pr.CreatedAt >= searchDto.FromDate.Value);
			}

			if (searchDto.ToDate.HasValue)
			{
				query = query.Where(pr => pr.CreatedAt <= searchDto.ToDate.Value);
			}

			return await query
				.OrderByDescending(pr => pr.CreatedAt)
				.Skip((searchDto.Page - 1) * searchDto.PageSize)
				.Take(searchDto.PageSize)
				.ToListAsync();
		}

		public async Task<int> GetTotalPaymentRequestsCountAsync(PaymentRequestSearchDTO searchDto)
		{
			var query = _context.PaymentRequests.AsQueryable();

			if (!string.IsNullOrEmpty(searchDto.TransferCode))
			{
				query = query.Where(pr => pr.TransferCode.Contains(searchDto.TransferCode));
			}

			if (!string.IsNullOrEmpty(searchDto.Status))
			{
				query = query.Where(pr => pr.Status == searchDto.Status);
			}

			if (searchDto.FromDate.HasValue)
			{
				query = query.Where(pr => pr.CreatedAt >= searchDto.FromDate.Value);
			}

			if (searchDto.ToDate.HasValue)
			{
				query = query.Where(pr => pr.CreatedAt <= searchDto.ToDate.Value);
			}

			return await query.CountAsync();
		}

		public async Task<bool> HasPendingPaymentRequestAsync(string userId, int planId)
		{
			return await _context.PaymentRequests
				.AnyAsync(pr => pr.UserId == userId &&
							   pr.PlanId == planId &&
							   pr.Status == "Pending" &&
							   pr.ExpiresAt > DateTime.UtcNow);
		}

		public async Task MarkAsExpiredAsync(int paymentRequestId)
		{
			var paymentRequest = await _context.PaymentRequests.FindAsync(paymentRequestId);
			if (paymentRequest != null && paymentRequest.Status == "Pending")
			{
				paymentRequest.Status = "Expired";
				_context.PaymentRequests.Update(paymentRequest);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<PaymentRequest>> GetRequestsToExpireAsync()
		{
			return await _context.PaymentRequests
				.Where(pr => pr.Status == "Pending" && pr.ExpiresAt <= DateTime.UtcNow)
				.ToListAsync();
		}
	}
}
