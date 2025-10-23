using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Models.CRM;

namespace WebApplication1.Services.CRM
{
    public interface ITaskService
    {
        Task<PagedResult<TaskItem>> SearchAsync(Guid? companyId, Guid? quoteId, string assignee, TaskStatus? status, DateTime? dueFrom, DateTime? dueTo, int page, int size);
        Task<TaskItem> GetAsync(Guid id);
        Task<TaskItem> CreateAsync(TaskItem task, string userId);
        Task<TaskItem> UpdateAsync(TaskItem task, string userId);
        Task MoveAsync(Guid id, TaskStatus toStatus, string userId);
        Task<TaskComment> AddCommentAsync(Guid taskId, TaskComment comment, string userId);
    }

    public interface IKanbanService
    {
        Task<IReadOnlyCollection<KanbanColumn>> GetColumnsAsync();
    }
}
