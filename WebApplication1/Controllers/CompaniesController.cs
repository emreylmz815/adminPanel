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
    public class CompaniesController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IContactService _contactService;
        private readonly ICrmNoteService _noteService;

        public CompaniesController()
        {
            _companyService = new InMemoryCompanyService();
            _contactService = new InMemoryContactService();
            _noteService = new InMemoryCrmNoteService();
        }

        public async Task<ActionResult> Index(string search, string tag, bool? active, int page = 1, int size = 20)
        {
            var model = new CompanyListViewModel
            {
                Search = search,
                Tag = tag,
                Active = active,
                Page = page,
                Size = size,
                Companies = await _companyService.SearchAsync(search, tag, active, page, size)
            };
            return View(model);
        }

        public ActionResult Create()
        {
            var company = new Company { IsActive = true };
            var model = new CompanyFormViewModel { Company = company };
            model.SyncTagsFromCompany();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CompanyFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.ApplyTagsToCompany();
            await _companyService.CreateAsync(model.Company, User?.Identity?.Name ?? "system");
            TempData["Message"] = "Company created";
            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> Edit(Guid id)
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
            {
                return HttpNotFound();
            }

            var model = new CompanyFormViewModel { Company = company };
            model.SyncTagsFromCompany();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Guid id, CompanyFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Company.Id = id;
            model.ApplyTagsToCompany();
            await _companyService.UpdateAsync(model.Company, User?.Identity?.Name ?? "system");
            TempData["Message"] = "Company updated";
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<ActionResult> Details(Guid id)
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
            {
                return HttpNotFound();
            }

            var contacts = await _contactService.GetByCompanyAsync(id);
            var notes = await _noteService.GetByCompanyAsync(id);
            var model = new CompanyDetailsViewModel
            {
                Company = company,
                Contacts = contacts,
                Notes = notes,
                ContactForm = new ContactFormViewModel { CompanyId = id },
                NoteForm = new NoteFormViewModel { CompanyId = id }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(Guid id)
        {
            await _companyService.DeactivateAsync(id, User?.Identity?.Name ?? "system");
            TempData["Message"] = "Company deactivated";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BulkTags(CompanyBulkTagsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid request";
                return RedirectToAction(nameof(Index));
            }

            await _companyService.BulkUpdateTagsAsync(model.CompanyIds, model.GetTagsToAdd(), model.GetTagsToRemove(), User?.Identity?.Name ?? "system");
            TempData["Message"] = "Tags updated";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveContact([Bind(Prefix = "ContactForm")] ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Details), new { id = model.CompanyId });
            }

            if (model.Id.HasValue)
            {
                var contact = await _contactService.GetByIdAsync(model.Id.Value);
                if (contact == null)
                {
                    return HttpNotFound();
                }

                contact.FullName = model.FullName;
                contact.Title = model.Title;
                contact.Email = model.Email;
                contact.Phone = model.Phone;
                contact.Notes = model.Notes;
                contact.IsPrimary = model.IsPrimary;
                await _contactService.UpdateAsync(contact);
                TempData["Message"] = "Contact updated";
            }
            else
            {
                var contact = new Contact
                {
                    CompanyId = model.CompanyId,
                    FullName = model.FullName,
                    Title = model.Title,
                    Email = model.Email,
                    Phone = model.Phone,
                    Notes = model.Notes,
                    IsPrimary = model.IsPrimary
                };
                await _contactService.CreateAsync(contact, User?.Identity?.Name ?? "system");
                TempData["Message"] = "Contact created";
            }

            return RedirectToAction(nameof(Details), new { id = model.CompanyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteContact(Guid id, Guid companyId)
        {
            await _contactService.DeleteAsync(id);
            TempData["Message"] = "Contact deleted";
            return RedirectToAction(nameof(Details), new { id = companyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddNote([Bind(Prefix = "NoteForm")] NoteFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Note content is required";
                return RedirectToAction(nameof(Details), new { id = model.CompanyId ?? Guid.Empty });
            }

            var note = new CrmNote
            {
                CompanyId = model.CompanyId,
                ContactId = model.ContactId,
                Content = model.Content,
                Pinned = model.Pinned
            };
            await _noteService.CreateAsync(note, User?.Identity?.Name ?? "system");
            TempData["Message"] = "Note added";
            return RedirectToAction(nameof(Details), new { id = model.CompanyId ?? Guid.Empty });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PinNote(Guid id, Guid companyId, bool pinned)
        {
            await _noteService.PinAsync(id, pinned);
            TempData["Message"] = pinned ? "Note pinned" : "Note unpinned";
            return RedirectToAction(nameof(Details), new { id = companyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteNote(Guid id, Guid companyId)
        {
            await _noteService.DeleteAsync(id);
            TempData["Message"] = "Note deleted";
            return RedirectToAction(nameof(Details), new { id = companyId });
        }
    }
}
