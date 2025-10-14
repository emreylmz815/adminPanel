using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebApplication1.Models
{
    public class ProposalProductInput
    {
        public ProposalProductInput()
        {
            VatRate = 20m;
            Quantity = 1;
        }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [Display(Name = "Ürün Adı")]
        public string Name { get; set; }

        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Adet 1 veya daha büyük olmalıdır.")]
        [Display(Name = "Adet")]
        public int Quantity { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Birim fiyat sıfırdan küçük olamaz.")]
        [Display(Name = "Birim Fiyat (₺)")]
        public decimal UnitPrice { get; set; }

        [Range(typeof(decimal), "0", "100", ErrorMessage = "KDV oranı 0-100 arasında olmalıdır.")]
        [Display(Name = "KDV (%)")]
        public decimal VatRate { get; set; }

        public decimal NetTotal => Math.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);

        public decimal VatAmount => Math.Round(NetTotal * (VatRate / 100m), 2, MidpointRounding.AwayFromZero);

        public decimal GrossTotal => NetTotal + VatAmount;
    }

    public class ProposalFormViewModel
    {
        public ProposalFormViewModel()
        {
            ProposalDate = DateTime.Now;
            ProposalNumber = $"TFK-{DateTime.UtcNow:yyyyMMddHHmmss}";
            Products = new List<ProposalProductInput> { new ProposalProductInput() };
        }

        [Display(Name = "Teklif Tarihi")]
        [DataType(DataType.Date)]
        public DateTime ProposalDate { get; set; }

        [Display(Name = "Teklif Numarası")]
        public string ProposalNumber { get; set; }

        [Required(ErrorMessage = "Firma adı zorunludur.")]
        [Display(Name = "Firma Adı")]
        public string CompanyName { get; set; }

        [Display(Name = "Firma Adresi")]
        public string CompanyAddress { get; set; }

        [Display(Name = "Firma Logosu (URL)")]
        public string CompanyLogoUrl { get; set; }

        [Display(Name = "Kaşe Bilgisi")]
        public string CompanyStampText { get; set; }

        [Display(Name = "Yetkili İsim")]
        public string AuthorizedSignatureName { get; set; }

        [Display(Name = "Yetkili Ünvanı")]
        public string AuthorizedSignatureTitle { get; set; }

        [Display(Name = "İmza Görseli (URL)")]
        public string AuthorizedSignatureImageUrl { get; set; }

        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        [Display(Name = "Müşteri Adı")]
        public string ClientName { get; set; }

        [Display(Name = "Müşteri Adresi")]
        public string ClientAddress { get; set; }

        [Display(Name = "Notlar")]
        public string Notes { get; set; }

        public List<ProposalProductInput> Products { get; set; }

        public ProposalSummaryViewModel ToSummary()
        {
            var summary = new ProposalSummaryViewModel
            {
                ProposalDate = ProposalDate,
                ProposalNumber = ProposalNumber,
                CompanyName = CompanyName,
                CompanyAddress = CompanyAddress,
                CompanyLogoUrl = CompanyLogoUrl,
                CompanyStampText = CompanyStampText,
                AuthorizedSignatureName = AuthorizedSignatureName,
                AuthorizedSignatureTitle = AuthorizedSignatureTitle,
                AuthorizedSignatureImageUrl = AuthorizedSignatureImageUrl,
                ClientName = ClientName,
                ClientAddress = ClientAddress,
                Notes = Notes
            };

            var items = Products ?? new List<ProposalProductInput>();
            foreach (var product in items)
            {
                summary.Products.Add(new ProposalProductSummary
                {
                    Name = product.Name,
                    Description = product.Description,
                    Quantity = product.Quantity,
                    UnitPrice = product.UnitPrice,
                    VatRate = product.VatRate,
                    NetTotal = product.NetTotal,
                    VatAmount = product.VatAmount,
                    GrossTotal = product.GrossTotal
                });
            }

            summary.TotalNet = Math.Round(summary.Products.Sum(p => p.NetTotal), 2, MidpointRounding.AwayFromZero);
            summary.TotalVat = Math.Round(summary.Products.Sum(p => p.VatAmount), 2, MidpointRounding.AwayFromZero);
            summary.TotalGross = Math.Round(summary.Products.Sum(p => p.GrossTotal), 2, MidpointRounding.AwayFromZero);

            summary.ProductCount = summary.Products.Count;

            return summary;
        }
    }

    public class ProposalProductSummary
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal VatRate { get; set; }
        public decimal NetTotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrossTotal { get; set; }
    }

    public class ProposalSummaryViewModel
    {
        public ProposalSummaryViewModel()
        {
            Products = new List<ProposalProductSummary>();
        }

        public DateTime ProposalDate { get; set; }
        public string ProposalNumber { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyLogoUrl { get; set; }
        public string CompanyStampText { get; set; }
        public string AuthorizedSignatureName { get; set; }
        public string AuthorizedSignatureTitle { get; set; }
        public string AuthorizedSignatureImageUrl { get; set; }
        public string ClientName { get; set; }
        public string ClientAddress { get; set; }
        public string Notes { get; set; }
        public List<ProposalProductSummary> Products { get; set; }
        public int ProductCount { get; set; }
        public decimal TotalNet { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalGross { get; set; }
    }
}
