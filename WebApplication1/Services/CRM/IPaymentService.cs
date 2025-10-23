using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Models.CRM;

namespace WebApplication1.Services.CRM
{
    public class PaymentSummary
    {
        public decimal Planned { get; set; }
        public decimal Paid { get; set; }
        public decimal Overdue { get; set; }
    }

    public interface IPaymentService
    {
        Task<PagedResult<PaymentPlan>> SearchPlansAsync(PaymentPlanStatus? status, Guid? companyId, Guid? quoteId, DateTime? from, DateTime? to, int page, int size);
        Task<PaymentPlan> GetPlanAsync(Guid id);
        Task<PaymentPlan> CreatePlanAsync(PaymentPlan plan, string userId);
        Task<PaymentPlan> UpdatePlanAsync(PaymentPlan plan, string userId);
        Task DeletePlanAsync(Guid id);
        Task<PaymentReceipt> PayAsync(Guid planId, decimal amount, PaymentMethod method, DateTime paidAt, string referenceNo, string notes, string userId);
        Task<IReadOnlyCollection<PaymentReceipt>> GetReceiptsAsync(Guid planId);
        Task<PaymentSummary> GetSummaryAsync(DateTime? from, DateTime? to);
    }
}
