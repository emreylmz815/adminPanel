using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebApplication1.Models.CRM;
using WebApplication1.Models.CRM.ViewModels;
using WebApplication1.Services.CRM;
using WebApplication1.Services.CRM.InMemory;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "admin,sales,ops,viewer")]
    public class TasksController : Controller
    {
        private readonly InMemoryTaskService _taskService;

        public TasksController()
        {
            _taskService = new InMemoryTaskService();
        }

        public async Task<ActionResult> Kanban()
        {
            var columns = await _taskService.GetColumnsAsync();
            var tasks = InMemoryCrmDataStore.Tasks.ToList();
            var model = new TaskKanbanViewModel
            {
                Columns = columns,
                Tasks = tasks
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Move(Guid id, TaskStatus toStatus)
        {
            await _taskService.MoveAsync(id, toStatus, User?.Identity?.Name ?? "system");
            return Json(new { success = true });
        }

        public async Task<ActionResult> List(Guid? companyId, Guid? quoteId, string assignee, TaskStatus? status, DateTime? dueFrom, DateTime? dueTo, int page = 1, int size = 20)
        {
            var tasks = await _taskService.SearchAsync(companyId, quoteId, assignee, status, dueFrom, dueTo, page, size);
            var model = new TaskListViewModel
            {
                CompanyId = companyId,
                QuoteId = quoteId,
                Assignee = assignee,
                Status = status,
                DueFrom = dueFrom,
                DueTo = dueTo,
                Page = page,
                Size = size,
                Tasks = tasks
            };
            return View(model);
        }

        public ActionResult Create()
        {
            var model = new TaskFormViewModel
            {
                Task = new TaskItem(),
                Companies = InMemoryCrmDataStore.Companies.ToList(),
                Quotes = InMemoryCrmDataStore.Quotes.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TaskFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Companies = InMemoryCrmDataStore.Companies.ToList();
                model.Quotes = InMemoryCrmDataStore.Quotes.ToList();
                return View(model);
            }

            await _taskService.CreateAsync(model.Task, User?.Identity?.Name ?? "system");
            TempData["Message"] = "Task created";
            return RedirectToAction(nameof(List));
        }

        public async Task<ActionResult> Edit(Guid id)
        {
            var task = await _taskService.GetAsync(id);
            if (task == null)
            {
                return HttpNotFound();
            }

            var model = new TaskFormViewModel
            {
                Task = task,
                Companies = InMemoryCrmDataStore.Companies.ToList(),
                Quotes = InMemoryCrmDataStore.Quotes.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Guid id, TaskFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Companies = InMemoryCrmDataStore.Companies.ToList();
                model.Quotes = InMemoryCrmDataStore.Quotes.ToList();
                return View(model);
            }

            model.Task.Id = id;
            await _taskService.UpdateAsync(model.Task, User?.Identity?.Name ?? "system");
            TempData["Message"] = "Task updated";
            return RedirectToAction(nameof(List));
        }

        public async Task<ActionResult> Details(Guid id)
        {
            var task = await _taskService.GetAsync(id);
            if (task == null)
            {
                return HttpNotFound();
            }

            var model = new TaskFormViewModel
            {
                Task = task,
                Companies = InMemoryCrmDataStore.Companies.ToList(),
                Quotes = InMemoryCrmDataStore.Quotes.ToList()
            };
            ViewBag.CommentForm = new TaskCommentViewModel();
            ViewBag.Comments = InMemoryCrmDataStore.TaskComments.Where(c => c.TaskId == id).OrderByDescending(c => c.CreatedAt).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Comment(Guid id, TaskCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Comment cannot be empty";
                return RedirectToAction(nameof(Details), new { id });
            }

            var comment = new TaskComment { Content = model.Content };
            await _taskService.AddCommentAsync(id, comment, User?.Identity?.Name ?? "system");
            TempData["Message"] = "Comment added";
            return RedirectToAction(nameof(Details), new { id });
        }

        public ActionResult Calendar(DateTime? start, DateTime? end)
        {
            var tasks = InMemoryCrmDataStore.Tasks.ToList();
            var events = tasks.Select(t => new
            {
                id = t.Id,
                title = t.Title,
                start = (t.DueDate ?? t.CreatedAt).ToString("o"),
                status = t.Status.ToString()
            });
            return Json(events, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Guid id)
        {
            var task = await _taskService.GetAsync(id);
            if (task != null)
            {
                InMemoryCrmDataStore.Tasks.Remove(task);
            }

            TempData["Message"] = "Task deleted";
            return RedirectToAction(nameof(List));
        }
    }
}
