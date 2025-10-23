using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models.CRM;

namespace WebApplication1.Services.CRM.InMemory
{
    public class InMemoryTaskService : ITaskService, IKanbanService
    {
        public InMemoryTaskService()
        {
            InMemoryCrmDataStore.EnsureSeeded();
        }

        public Task<PagedResult<TaskItem>> SearchAsync(Guid? companyId, Guid? quoteId, string assignee, TaskStatus? status, DateTime? dueFrom, DateTime? dueTo, int page, int size)
        {
            var query = InMemoryCrmDataStore.Tasks.AsQueryable();

            if (companyId.HasValue)
            {
                query = query.Where(t => t.CompanyId == companyId);
            }

            if (quoteId.HasValue)
            {
                query = query.Where(t => t.QuoteId == quoteId);
            }

            if (!string.IsNullOrWhiteSpace(assignee))
            {
                query = query.Where(t => string.Equals(t.AssigneeUserId, assignee, StringComparison.OrdinalIgnoreCase));
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status);
            }

            if (dueFrom.HasValue)
            {
                query = query.Where(t => t.DueDate >= dueFrom);
            }

            if (dueTo.HasValue)
            {
                query = query.Where(t => t.DueDate <= dueTo);
            }

            var total = query.Count();
            var items = query
                .OrderBy(t => t.DueDate ?? DateTime.MaxValue)
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();

            return Task.FromResult(new PagedResult<TaskItem>(items, page, size, total));
        }

        public Task<TaskItem> GetAsync(Guid id)
        {
            var task = InMemoryCrmDataStore.Tasks.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(task);
        }

        public Task<TaskItem> CreateAsync(TaskItem task, string userId)
        {
            task.Id = Guid.NewGuid();
            task.CreatedAt = DateTime.UtcNow;
            task.CreatedBy = userId;
            InMemoryCrmDataStore.Tasks.Add(task);
            return Task.FromResult(task);
        }

        public Task<TaskItem> UpdateAsync(TaskItem task, string userId)
        {
            var existing = InMemoryCrmDataStore.Tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existing == null)
            {
                throw new InvalidOperationException("Task not found");
            }

            existing.Title = task.Title;
            existing.Description = task.Description;
            existing.CompanyId = task.CompanyId;
            existing.QuoteId = task.QuoteId;
            existing.AssigneeUserId = task.AssigneeUserId;
            existing.Status = task.Status;
            existing.Priority = task.Priority;
            existing.DueDate = task.DueDate;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = userId;

            return Task.FromResult(existing);
        }

        public Task MoveAsync(Guid id, TaskStatus toStatus, string userId)
        {
            var task = InMemoryCrmDataStore.Tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                throw new InvalidOperationException("Task not found");
            }

            task.Status = toStatus;
            task.UpdatedAt = DateTime.UtcNow;
            task.UpdatedBy = userId;
            return Task.CompletedTask;
        }

        public Task<TaskComment> AddCommentAsync(Guid taskId, TaskComment comment, string userId)
        {
            var task = InMemoryCrmDataStore.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                throw new InvalidOperationException("Task not found");
            }

            comment.Id = Guid.NewGuid();
            comment.TaskId = taskId;
            comment.CreatedAt = DateTime.UtcNow;
            comment.CreatedBy = userId;

            InMemoryCrmDataStore.TaskComments.Add(comment);
            task.Comments.Add(comment);
            return Task.FromResult(comment);
        }

        public Task<IReadOnlyCollection<KanbanColumn>> GetColumnsAsync()
        {
            var columns = InMemoryCrmDataStore.KanbanColumns
                .OrderBy(c => c.DisplayOrder)
                .ToList();
            return Task.FromResult((IReadOnlyCollection<KanbanColumn>)columns);
        }
    }
}
