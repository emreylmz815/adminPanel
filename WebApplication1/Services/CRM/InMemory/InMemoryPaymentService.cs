using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models.CRM;

namespace WebApplication1.Services.CRM.InMemory
{
    public class InMemoryPaymentService : IPaymentService
    {
        public InMemoryPaymentService()
        {
            InMemoryCrmDataStore.EnsureSeeded();
        }

        public Task<PagedResult<PaymentPlan>> SearchPlansAsync(PaymentPlanStatus? status, Guid? companyId, Guid? quoteId, DateTime? from, DateTime? to, int page, int size)
        {
            var query = InMemoryCrmDataStore.PaymentPlans.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status);
            }

            if (quoteId.HasValue)
            {
                query = query.Where(p => p.QuoteId == quoteId);
            }

            if (companyId.HasValue)
            {
                var quoteIds = InMemoryCrmDataStore.Quotes.Where(q => q.CompanyId == companyId).Select(q => q.Id).ToArray();
                query = query.Where(p => quoteIds.Contains(p.QuoteId));
            }

            if (from.HasValue)
            {
                query = query.Where(p => p.DueDate >= from.Value.Date);
            }

            if (to.HasValue)
            {
                query = query.Where(p => p.DueDate <= to.Value.Date);
            }

            var total = query.Count();
            var items = query
                .OrderBy(p => p.DueDate)
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();

            return Task.FromResult(new PagedResult<PaymentPlan>(items, page, size, total));
        }

        public Task<PaymentPlan> GetPlanAsync(Guid id)
        {
            var plan = InMemoryCrmDataStore.PaymentPlans.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(plan);
        }

        public Task<PaymentPlan> CreatePlanAsync(PaymentPlan plan, string userId)
        {
            plan.Id = Guid.NewGuid();
            InMemoryCrmDataStore.PaymentPlans.Add(plan);
            return Task.FromResult(plan);
        }

        public Task<PaymentPlan> UpdatePlanAsync(PaymentPlan plan, string userId)
        {
            var existing = InMemoryCrmDataStore.PaymentPlans.FirstOrDefault(p => p.Id == plan.Id);
            if (existing == null)
            {
                throw new InvalidOperationException("Payment plan not found");
            }

            existing.DueDate = plan.DueDate;
            existing.Amount = plan.Amount;
            existing.Currency = plan.Currency;
            existing.Status = plan.Status;
            existing.Notes = plan.Notes;
            return Task.FromResult(existing);
        }

        public Task DeletePlanAsync(Guid id)
        {
            var plan = InMemoryCrmDataStore.PaymentPlans.FirstOrDefault(p => p.Id == id);
            if (plan != null)
            {
                InMemoryCrmDataStore.PaymentPlans.Remove(plan);
            }

            return Task.CompletedTask;
        }

        public Task<PaymentReceipt> PayAsync(Guid planId, decimal amount, PaymentMethod method, DateTime paidAt, string referenceNo, string notes, string userId)
        {
            var plan = InMemoryCrmDataStore.PaymentPlans.FirstOrDefault(p => p.Id == planId);
            if (plan == null)
            {
                throw new InvalidOperationException("Payment plan not found");
            }

            var totalPaid = InMemoryCrmDataStore.PaymentReceipts.Where(r => r.PaymentPlanId == planId).Sum(r => r.Amount);
            if (totalPaid + amount > plan.Amount)
            {
                throw new InvalidOperationException("Payment exceeds planned amount");
            }

            var receipt = new PaymentReceipt
            {
                Id = Guid.NewGuid(),
                PaymentPlanId = planId,
                PaidAt = paidAt,
                Amount = amount,
                Method = method,
                ReferenceNo = referenceNo,
                Notes = notes
            };

            InMemoryCrmDataStore.PaymentReceipts.Add(receipt);
            plan.Receipts.Add(receipt);

            totalPaid += amount;
            plan.Status = totalPaid >= plan.Amount
                ? PaymentPlanStatus.Paid
                : plan.DueDate < DateTime.UtcNow.Date ? PaymentPlanStatus.Overdue : PaymentPlanStatus.Planned;

            return Task.FromResult(receipt);
        }

        public Task<IReadOnlyCollection<PaymentReceipt>> GetReceiptsAsync(Guid planId)
        {
            var receipts = InMemoryCrmDataStore.PaymentReceipts
                .Where(r => r.PaymentPlanId == planId)
                .OrderByDescending(r => r.PaidAt)
                .ToList();
            return Task.FromResult((IReadOnlyCollection<PaymentReceipt>)receipts);
        }

        public Task<PaymentSummary> GetSummaryAsync(DateTime? from, DateTime? to)
        {
            var plans = InMemoryCrmDataStore.PaymentPlans.AsEnumerable();
            if (from.HasValue)
            {
                plans = plans.Where(p => p.DueDate >= from.Value.Date);
            }

            if (to.HasValue)
            {
                plans = plans.Where(p => p.DueDate <= to.Value.Date);
            }

            var summary = new PaymentSummary
            {
                Planned = plans.Where(p => p.Status == PaymentPlanStatus.Planned).Sum(p => p.Amount),
                Paid = plans.Where(p => p.Status == PaymentPlanStatus.Paid).Sum(p => p.Amount),
                Overdue = plans.Where(p => p.Status == PaymentPlanStatus.Overdue).Sum(p => p.Amount)
            };

            return Task.FromResult(summary);
        }
    }
}
