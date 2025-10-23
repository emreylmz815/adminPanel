using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace WebApplication1.Models.CRM
{
    /// <summary>
    /// Represents a tenant scoped company inside the lightweight CRM module.
    /// </summary>
    [Table("CrmCompanies")]
    public class Company
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(120, MinimumLength = 2)]
        public string Name { get; set; }

        [StringLength(11, MinimumLength = 10)]
        public string TaxNo { get; set; }

        [StringLength(120)]
        public string Sector { get; set; }

        [StringLength(32)]
        public string Phone { get; set; }

        [EmailAddress]
        [StringLength(160)]
        public string Email { get; set; }

        [Url]
        [StringLength(160)]
        public string Website { get; set; }

        [StringLength(512)]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the raw JSON representation of the tag list.
        /// </summary>
        public string Tags { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(64)]
        public string CreatedBy { get; set; }

        [StringLength(64)]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Convenience accessor that exposes the JSON tag payload as a simple array.
        /// </summary>
        [NotMapped]
        public string[] TagList
        {
            get => string.IsNullOrWhiteSpace(Tags)
                ? Array.Empty<string>()
                : JsonConvert.DeserializeObject<string[]>(Tags) ?? Array.Empty<string>();
            set => Tags = value == null || value.Length == 0
                ? null
                : JsonConvert.SerializeObject(value);
        }
    }
}
