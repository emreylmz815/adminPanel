using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.CRM
{
    public enum PaymentMethod
    {
        Cash,
        Bank,
        Pos,
        Transfer
    }

    [Table("CrmPaymentReceipts")]
    public class PaymentReceipt
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PaymentPlanId { get; set; }

        [ForeignKey(nameof(PaymentPlanId))]
        public PaymentPlan PaymentPlan { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public PaymentMethod Method { get; set; }

        [StringLength(64)]
        public string ReferenceNo { get; set; }

        [StringLength(1024)]
        public string Notes { get; set; }
    }
}
