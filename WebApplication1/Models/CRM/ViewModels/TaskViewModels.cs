using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Services.CRM;

namespace WebApplication1.Models.CRM.ViewModels
{
    public class TaskListViewModel
    {
        public Guid? CompanyId { get; set; }
        public Guid? QuoteId { get; set; }
        public string Assignee { get; set; }
        public TaskStatus? Status { get; set; }
        public DateTime? DueFrom { get; set; }
        public DateTime? DueTo { get; set; }
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 20;
        public PagedResult<TaskItem> Tasks { get; set; }
    }

    public class TaskFormViewModel
    {
        public TaskItem Task { get; set; }
        public IReadOnlyCollection<Company> Companies { get; set; }
        public IReadOnlyCollection<Quote> Quotes { get; set; }
    }

    public class TaskKanbanViewModel
    {
        public IReadOnlyCollection<KanbanColumn> Columns { get; set; }
        public IReadOnlyCollection<TaskItem> Tasks { get; set; }
    }

    public class TaskCommentViewModel
    {
        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Content { get; set; }
    }
}
