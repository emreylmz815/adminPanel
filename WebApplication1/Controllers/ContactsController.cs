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
    public class ContactsController : Controller
    {
        private readonly IContactService _contactService;

        public ContactsController()
        {
            _contactService = new InMemoryContactService();
        }

        public async Task<ActionResult> Index(Guid companyId)
        {
            var contacts = await _contactService.GetByCompanyAsync(companyId);
            ViewBag.CompanyId = companyId;
            return PartialView("_ContactTable", contacts);
        }

        public ActionResult Create(Guid companyId)
        {
            var model = new ContactFormViewModel { CompanyId = companyId };
            return PartialView("_ContactForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_ContactForm", model);
            }

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
            var contacts = await _contactService.GetByCompanyAsync(model.CompanyId);
            return PartialView("_ContactTable", contacts);
        }

        public async Task<ActionResult> Edit(Guid id)
        {
            var contact = await _contactService.GetByIdAsync(id);
            if (contact == null)
            {
                return HttpNotFound();
            }

            var model = new ContactFormViewModel
            {
                CompanyId = contact.CompanyId,
                Id = contact.Id,
                FullName = contact.FullName,
                Title = contact.Title,
                Email = contact.Email,
                Phone = contact.Phone,
                Notes = contact.Notes,
                IsPrimary = contact.IsPrimary
            };
            return PartialView("_ContactForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ContactFormViewModel model)
        {
            if (!ModelState.IsValid || !model.Id.HasValue)
            {
                return PartialView("_ContactForm", model);
            }

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
            var contacts = await _contactService.GetByCompanyAsync(model.CompanyId);
            return PartialView("_ContactTable", contacts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Guid id, Guid companyId)
        {
            await _contactService.DeleteAsync(id);
            var contacts = await _contactService.GetByCompanyAsync(companyId);
            return PartialView("_ContactTable", contacts);
        }
    }
}
