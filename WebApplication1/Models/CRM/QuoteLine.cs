using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.CRM
{
    [Table("CrmQuoteLines")]
    public class QuoteLine
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid QuoteId { get; set; }

        [ForeignKey(nameof(QuoteId))]
        public Quote Quote { get; set; }

        [StringLength(64)]
        public string Sku { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Range(0.01, 999999)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Qty { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? LineCost { get; set; }

        [NotMapped]
        public decimal LineTotal { get; private set; }

        public void Recalculate()
        {
            LineTotal = Math.Round(Qty * UnitPrice, 2, MidpointRounding.AwayFromZero);
        }
    }
}
