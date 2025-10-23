using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.CRM
{
    [Table("CrmContacts")]
    public class Contact
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; }

        [Required]
        [StringLength(120, MinimumLength = 2)]
        public string FullName { get; set; }

        [StringLength(120)]
        public string Title { get; set; }

        [EmailAddress]
        [StringLength(160)]
        public string Email { get; set; }

        [StringLength(32)]
        public string Phone { get; set; }

        [StringLength(1024)]
        public string Notes { get; set; }

        public bool IsPrimary { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(64)]
        public string CreatedBy { get; set; }
    }
}
