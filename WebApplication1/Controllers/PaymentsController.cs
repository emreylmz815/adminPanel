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
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController()
        {
            _paymentService = new InMemoryPaymentService();
        }

        public async Task<ActionResult> Plans(PaymentPlanStatus? status, Guid? companyId, Guid? quoteId, DateTime? from, DateTime? to, int page = 1, int size = 20)
        {
            var plans = await _paymentService.SearchPlansAsync(status, companyId, quoteId, from, to, page, size);
            var summary = await _paymentService.GetSummaryAsync(from, to);
            var model = new PaymentListViewModel
            {
                Status = status,
                CompanyId = companyId,
                QuoteId = quoteId,
                From = from,
                To = to,
                Page = page,
                Size = size,
                Plans = plans,
                Summary = summary,
                Quotes = InMemoryCrmDataStore.Quotes.ToList(),
                Companies = InMemoryCrmDataStore.Companies.ToList()
            };
            return View("Index", model);
        }

        public ActionResult CreatePlan()
        {
            var model = new PaymentPlanFormViewModel
            {
                Plan = new PaymentPlan { DueDate = DateTime.UtcNow.Date.AddDays(7) },
                Quotes = InMemoryCrmDataStore.Quotes.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatePlan(PaymentPlanFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Quotes = InMemoryCrmDataStore.Quotes.ToList();
                return View(model);
            }

            await _paymentService.CreatePlanAsync(model.Plan, User?.Identity?.Name ?? "system");
            TempData["Message"] = "Payment plan created";
            return RedirectToAction(nameof(Plans));
        }

        public async Task<ActionResult> EditPlan(Guid id)
        {
            var plan = await _paymentService.GetPlanAsync(id);
            if (plan == null)
            {
                return HttpNotFound();
            }

            var model = new PaymentPlanFormViewModel
            {
                Plan = plan,
                Quotes = InMemoryCrmDataStore.Quotes.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPlan(Guid id, PaymentPlanFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Quotes = InMemoryCrmDataStore.Quotes.ToList();
                return View(model);
            }

            model.Plan.Id = id;
            await _paymentService.UpdatePlanAsync(model.Plan, User?.Identity?.Name ?? "system");
            TempData["Message"] = "Payment plan updated";
            return RedirectToAction(nameof(Plans));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePlan(Guid id)
        {
            await _paymentService.DeletePlanAsync(id);
            TempData["Message"] = "Payment plan deleted";
            return RedirectToAction(nameof(Plans));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Pay(PaymentReceiptFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid payment";
                return RedirectToAction(nameof(Plans));
            }

            await _paymentService.PayAsync(model.PlanId, model.Amount, model.Method, model.PaidAt, model.ReferenceNo, model.Notes, User?.Identity?.Name ?? "system");
            TempData["Message"] = "Payment recorded";
            return RedirectToAction(nameof(Plans));
        }

        public async Task<ActionResult> Receipts(Guid planId)
        {
            var receipts = await _paymentService.GetReceiptsAsync(planId);
            ViewBag.PlanId = planId;
            return PartialView("_ReceiptTable", receipts);
        }
    }
}
