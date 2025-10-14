using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Areas.admin.Controllers
{
    public class ProposalController : Controller
    {
        private readonly PdfProposalService _pdfProposalService;

        public ProposalController()
        {
            _pdfProposalService = new PdfProposalService();
        }

        [HttpGet]
        public ActionResult Create()
        {
            var model = new ProposalFormViewModel
            {
                CompanyName = "Şirket Adı",
                CompanyStampText = "Şirket Kaşesi",
                AuthorizedSignatureName = "Yetkili İsim",
                AuthorizedSignatureTitle = "Yetkili Ünvan",
                ClientName = "Müşteri Adı"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProposalFormViewModel model)
        {
            if (model.Products == null || !model.Products.Any())
            {
                ModelState.AddModelError("Products", "En az bir ürün eklemelisiniz.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var filteredProducts = model.Products.Where(p => !string.IsNullOrWhiteSpace(p.Name)).ToList();
            if (!filteredProducts.Any())
            {
                ModelState.AddModelError("Products", "Geçerli en az bir ürün eklemelisiniz.");
                model.Products = new List<ProposalProductInput> { new ProposalProductInput() };
                return View(model);
            }

            model.Products = filteredProducts;

            var summary = model.ToSummary();
            TempData["ProposalSummaryData"] = JsonSerializer.Serialize(summary);

            return RedirectToAction("Summary");
        }

        [HttpGet]
        public ActionResult Summary()
        {
            var json = TempData.Peek("ProposalSummaryData") as string;
            if (string.IsNullOrWhiteSpace(json))
            {
                return RedirectToAction("Create");
            }

            var summary = JsonSerializer.Deserialize<ProposalSummaryViewModel>(json);
            return View(summary);
        }

        [HttpGet]
        public ActionResult DownloadPdf()
        {
            var json = TempData.Peek("ProposalSummaryData") as string;
            if (string.IsNullOrWhiteSpace(json))
            {
                return RedirectToAction("Create");
            }

            var summary = JsonSerializer.Deserialize<ProposalSummaryViewModel>(json);
            var fileBytes = _pdfProposalService.GenerateProposalPdf(summary);
            var fileName = $"{summary.ProposalNumber ?? "Teklif"}.pdf";
            return File(fileBytes, "application/pdf", fileName);
        }
    }
}
