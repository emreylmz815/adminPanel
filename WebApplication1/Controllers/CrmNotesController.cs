using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebApplication1.Models.CRM;
using WebApplication1.Models.CRM.ViewModels;
using WebApplication1.Services.CRM;
using WebApplication1.Services.CRM.InMemory;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "admin,sales,ops,viewer")]
    public class CrmNotesController : Controller
    {
        private readonly ICrmNoteService _noteService;

        public CrmNotesController()
        {
            _noteService = new InMemoryCrmNoteService();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(NoteFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(400, "Invalid note");
            }

            var note = new CrmNote
            {
                CompanyId = model.CompanyId,
                ContactId = model.ContactId,
                Content = model.Content,
                Pinned = model.Pinned
            };
            await _noteService.CreateAsync(note, User?.Identity?.Name ?? "system");
            return RedirectToAction("Details", "Companies", new { id = model.CompanyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Pin(Guid id, Guid companyId, bool pinned)
        {
            await _noteService.PinAsync(id, pinned);
            return RedirectToAction("Details", "Companies", new { id = companyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Guid id, Guid companyId)
        {
            await _noteService.DeleteAsync(id);
            return RedirectToAction("Details", "Companies", new { id = companyId });
        }
    }
}
