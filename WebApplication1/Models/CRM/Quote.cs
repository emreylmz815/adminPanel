using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace WebApplication1.Models.CRM
{
    public enum QuoteStatus
    {
        Draft,
        Sent,
        Approved,
        Rejected,
        Expired
    }

    [Table("CrmQuotes")]
    public class Quote
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; }

        [Required]
        [StringLength(32)]
        public string No { get; set; }

        [Required]
        [StringLength(120, MinimumLength = 3)]
        public string Title { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Range(0, 30)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Range(0, 50)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostTotal { get; set; }

        [NotMapped]
        public decimal? ProfitRate => Total == 0 ? (decimal?)null : Math.Round((Total - CostTotal) / Total, 4);

        public QuoteStatus Status { get; set; } = QuoteStatus.Draft;

        public DateTime? SentAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [StringLength(64)]
        public string CreatedBy { get; set; }

        [InverseProperty(nameof(QuoteLine.Quote))]
        public ICollection<QuoteLine> Lines { get; set; } = new List<QuoteLine>();

        /// <summary>
        /// Recalculates the financial figures based on the current lines.
        /// </summary>
        public void RecalculateTotals()
        {
            foreach (var line in Lines)
            {
                line.Recalculate();
            }

            Subtotal = Lines.Sum(x => x.LineTotal);
            TaxAmount = Math.Round(Subtotal * (TaxRate / 100m), 2, MidpointRounding.AwayFromZero);
            var discountedSubtotal = Subtotal - Math.Round(Subtotal * (DiscountRate / 100m), 2, MidpointRounding.AwayFromZero);
            Total = discountedSubtotal + TaxAmount;
        }
    }
}
