using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.CRM
{
    public enum PaymentPlanStatus
    {
        Planned,
        Paid,
        Overdue
    }

    [Table("CrmPaymentPlans")]
    public class PaymentPlan
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid QuoteId { get; set; }

        [ForeignKey(nameof(QuoteId))]
        public Quote Quote { get; set; }

        [Column(TypeName = "date")]
        public DateTime DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(8)]
        public string Currency { get; set; } = "TRY";

        public PaymentPlanStatus Status { get; set; } = PaymentPlanStatus.Planned;

        [StringLength(1024)]
        public string Notes { get; set; }

        [InverseProperty(nameof(PaymentReceipt.PaymentPlan))]
        public virtual System.Collections.Generic.ICollection<PaymentReceipt> Receipts { get; set; } = new System.Collections.Generic.List<PaymentReceipt>();
    }
}
