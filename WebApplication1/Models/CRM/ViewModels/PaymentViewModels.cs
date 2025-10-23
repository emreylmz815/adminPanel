using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Services.CRM;

namespace WebApplication1.Models.CRM.ViewModels
{
    public class PaymentListViewModel
    {
        public PaymentPlanStatus? Status { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? QuoteId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 20;
        public PagedResult<PaymentPlan> Plans { get; set; }
        public PaymentSummary Summary { get; set; }
        public IReadOnlyCollection<Quote> Quotes { get; set; }
        public IReadOnlyCollection<Company> Companies { get; set; }
    }

    public class PaymentPlanFormViewModel
    {
        public PaymentPlan Plan { get; set; }
        public IReadOnlyCollection<Quote> Quotes { get; set; }
    }

    public class PaymentReceiptFormViewModel
    {
        public Guid PlanId { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "999999999999")] 
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [Required]
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        [StringLength(64)]
        public string ReferenceNo { get; set; }

        [StringLength(1024)]
        public string Notes { get; set; }
    }
}
