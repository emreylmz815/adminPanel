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
    public class QuotesController : Controller
    {
        private readonly IQuoteService _quoteService;

        public QuotesController()
        {
            var generator = new InMemoryQuoteNumberGenerator();
            _quoteService = new InMemoryQuoteService(generator);
        }

        public async Task<ActionResult> Index(QuoteStatus? status, Guid? companyId, string search, int page = 1, int size = 20)
        {
            var quotes = await _quoteService.SearchAsync(status, companyId, search, page, size);
            var companies = InMemoryCrmDataStore.Companies.ToList();
            var model = new QuoteListViewModel
            {
                Status = status,
                CompanyId = companyId,
                Search = search,
                Page = page,
                Size = size,
                Quotes = quotes,
                Companies = companies
            };
            return View(model);
        }

        public async Task<ActionResult> Details(Guid id)
        {
            var quote = await _quoteService.GetAsync(id);
            if (quote == null)
            {
                return HttpNotFound();
            }

            var companies = InMemoryCrmDataStore.Companies.ToList();
            var model = new QuoteFormViewModel
            {
                Quote = quote,
                Companies = companies
            };
            return View(model);
        }

        public async Task<ActionResult> Create()
        {
            var quote = new Quote
            {
                Lines =
                {
                    new QuoteLine(),
                    new QuoteLine()
                }
            };
            var companies = InMemoryCrmDataStore.Companies.ToList();
            var model = new QuoteFormViewModel
            {
                Quote = quote,
                Companies = companies
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(QuoteFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Companies = InMemoryCrmDataStore.Companies.ToList();
                return View(model);
            }

            model.Quote.Lines = model.Quote.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Name)).ToList();
            await _quoteService.CreateAsync(model.Quote, User?.Identity?.Name ?? "system");
            TempData["Message"] = "Quote created";
            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> Edit(Guid id)
        {
            var quote = await _quoteService.GetAsync(id);
            if (quote == null)
            {
                return HttpNotFound();
            }

            var companies = InMemoryCrmDataStore.Companies.ToList();
            var model = new QuoteFormViewModel { Quote = quote, Companies = companies };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Guid id, QuoteFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Companies = InMemoryCrmDataStore.Companies.ToList();
                return View(model);
            }

            model.Quote.Id = id;
            model.Quote.Lines = model.Quote.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Name)).ToList();
            await _quoteService.UpdateAsync(model.Quote, User?.Identity?.Name ?? "system");
            TempData["Message"] = "Quote updated";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Status(Guid id, string action)
        {
            var status = action?.ToLowerInvariant() switch
            {
                "send" => QuoteStatus.Sent,
                "approve" => QuoteStatus.Approved,
                "reject" => QuoteStatus.Rejected,
                "expire" => QuoteStatus.Expired,
                _ => QuoteStatus.Draft
            };

            await _quoteService.ChangeStatusAsync(id, status, User?.Identity?.Name ?? "system");
            TempData["Message"] = $"Quote status changed to {status}";
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<ActionResult> Pdf(Guid id)
        {
            var pdf = await _quoteService.GeneratePdfAsync(id);
            return File(pdf, "application/pdf", $"quote-{id}.pdf");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Email(QuoteEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid email";
                return RedirectToAction(nameof(Details), new { id = model.QuoteId });
            }

            await _quoteService.SendEmailAsync(model.QuoteId, model.To, model.Cc, model.Subject, model.Body, User?.Identity?.Name ?? "system");
            TempData["Message"] = "Email sent";
            return RedirectToAction(nameof(Details), new { id = model.QuoteId });
        }
    }
}
