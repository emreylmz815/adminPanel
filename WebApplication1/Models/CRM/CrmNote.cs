using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.CRM
{
    [Table("CrmNotes")]
    public class CrmNote
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? CompanyId { get; set; }

        public Guid? ContactId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; }

        [ForeignKey(nameof(ContactId))]
        public Contact Contact { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 1)]
        public string Content { get; set; }

        public bool Pinned { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(64)]
        public string CreatedBy { get; set; }
    }
}
