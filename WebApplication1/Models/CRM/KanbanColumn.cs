using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.CRM
{
    [Table("CrmKanbanColumns")]
    public class KanbanColumn
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(64)]
        public string Name { get; set; }

        public int DisplayOrder { get; set; }
    }
}
