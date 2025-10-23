using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.CRM
{
    public enum TaskStatus
    {
        Todo,
        InProgress,
        Waiting,
        Done
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High
    }

    [Table("CrmTasks")]
    public class TaskItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(160, MinimumLength = 2)]
        public string Title { get; set; }

        [StringLength(4000)]
        public string Description { get; set; }

        public Guid? CompanyId { get; set; }

        public Guid? QuoteId { get; set; }

        [StringLength(64)]
        public string AssigneeUserId { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.Todo;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(64)]
        public string CreatedBy { get; set; }

        [InverseProperty(nameof(TaskComment.Task))]
        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    }
}
