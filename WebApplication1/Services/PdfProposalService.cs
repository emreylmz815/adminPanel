using System;
using System.Globalization;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class PdfProposalService
    {
        public byte[] GenerateProposalPdf(ProposalSummaryViewModel summary)
        {
            if (summary == null)
            {
                throw new ArgumentNullException(nameof(summary));
            }

            using (var stream = new MemoryStream())
            {
                using (var document = new Document(PageSize.A4, 36, 36, 54, 36))
                {
                    PdfWriter.GetInstance(document, stream);
                    document.Open();

                    AddHeader(document, summary);
                    AddClientInfo(document, summary);
                    AddProductTable(document, summary);
                    AddTotals(document, summary);
                    AddFooter(document, summary);

                    document.Close();
                }

                return stream.ToArray();
            }
        }

        private static void AddHeader(Document document, ProposalSummaryViewModel summary)
        {
            var headerTable = new PdfPTable(2)
            {
                WidthPercentage = 100
            };
            headerTable.SetWidths(new[] { 2f, 1f });

            var companyInfo = new Paragraph(summary.CompanyName ?? string.Empty, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
            {
                Alignment = Element.ALIGN_LEFT
            };
            companyInfo.Add(new Chunk("\n"));
            companyInfo.Add(new Phrase(summary.CompanyAddress ?? string.Empty, FontFactory.GetFont(FontFactory.HELVETICA, 10)));

            var cell = new PdfPCell(companyInfo)
            {
                Border = Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            headerTable.AddCell(cell);

            if (!string.IsNullOrWhiteSpace(summary.CompanyLogoUrl))
            {
                try
                {
                    var logo = Image.GetInstance(summary.CompanyLogoUrl);
                    logo.ScaleToFit(120f, 60f);
                    var logoCell = new PdfPCell(logo)
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Border = Rectangle.NO_BORDER
                    };
                    headerTable.AddCell(logoCell);
                }
                catch
                {
                    headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
                }
            }
            else
            {
                headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
            }

            document.Add(headerTable);

            var infoTable = new PdfPTable(2)
            {
                WidthPercentage = 100,
                SpacingBefore = 15f
            };
            infoTable.SetWidths(new[] { 1f, 1f });

            infoTable.AddCell(CreateInfoCell("Teklif No", summary.ProposalNumber));
            infoTable.AddCell(CreateInfoCell("Teklif Tarihi", summary.ProposalDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)));
            infoTable.AddCell(CreateInfoCell("Ürün Sayısı", summary.ProductCount.ToString()));
            infoTable.AddCell(CreateInfoCell("Yetkili", string.IsNullOrWhiteSpace(summary.AuthorizedSignatureTitle)
                ? summary.AuthorizedSignatureName
                : $"{summary.AuthorizedSignatureName} ({summary.AuthorizedSignatureTitle})"));

            document.Add(infoTable);
        }

        private static PdfPCell CreateInfoCell(string title, string value)
        {
            var cell = new PdfPCell()
            {
                Border = Rectangle.NO_BORDER,
                PaddingBottom = 8f
            };
            var phrase = new Phrase();
            phrase.Add(new Chunk(title + ": ", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)));
            phrase.Add(new Chunk(value ?? "-", FontFactory.GetFont(FontFactory.HELVETICA, 10)));
            cell.AddElement(phrase);
            return cell;
        }

        private static void AddClientInfo(Document document, ProposalSummaryViewModel summary)
        {
            var title = new Paragraph("Müşteri Bilgileri", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12))
            {
                SpacingBefore = 10f,
                SpacingAfter = 4f
            };
            document.Add(title);

            var clientInfo = new Paragraph
            {
                Font = FontFactory.GetFont(FontFactory.HELVETICA, 10)
            };
            clientInfo.Add(new Phrase(summary.ClientName ?? string.Empty));
            clientInfo.Add(new Chunk("\n"));
            clientInfo.Add(new Phrase(summary.ClientAddress ?? string.Empty));
            document.Add(clientInfo);
        }

        private static void AddProductTable(Document document, ProposalSummaryViewModel summary)
        {
            var table = new PdfPTable(6)
            {
                WidthPercentage = 100,
                SpacingBefore = 15f
            };
            table.SetWidths(new[] { 2.5f, 3f, 1f, 1.2f, 1.2f, 1.4f });

            AddTableHeader(table, "Ürün", "Açıklama", "Adet", "Birim Fiyat", "KDV", "Toplam");

            foreach (var product in summary.Products)
            {
                table.AddCell(CreateTextCell(product.Name));
                table.AddCell(CreateTextCell(product.Description));
                table.AddCell(CreateTextCell(product.Quantity.ToString()));
                table.AddCell(CreateTextCell(product.UnitPrice.ToString("C", CultureInfo.GetCultureInfo("tr-TR"))));
                table.AddCell(CreateTextCell($"%{product.VatRate:0.##}\n({product.VatAmount.ToString("C", CultureInfo.GetCultureInfo("tr-TR"))})"));
                table.AddCell(CreateTextCell(product.GrossTotal.ToString("C", CultureInfo.GetCultureInfo("tr-TR"))));
            }

            document.Add(table);
        }

        private static void AddTotals(Document document, ProposalSummaryViewModel summary)
        {
            var totalsTable = new PdfPTable(2)
            {
                WidthPercentage = 40,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                SpacingBefore = 12f
            };
            totalsTable.SetWidths(new[] { 1f, 1f });

            totalsTable.AddCell(CreateTotalsCell("Ara Toplam", summary.TotalNet));
            totalsTable.AddCell(CreateTotalsCell("KDV Toplam", summary.TotalVat));
            totalsTable.AddCell(CreateTotalsCell("Genel Toplam", summary.TotalGross, isBold: true));

            document.Add(totalsTable);
        }

        private static PdfPCell CreateTotalsCell(string title, decimal value, bool isBold = false)
        {
            var font = isBold ? FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10) : FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var phrase = new Phrase
            {
                new Chunk(title + ": ", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)),
                new Chunk(value.ToString("C", CultureInfo.GetCultureInfo("tr-TR")), font)
            };
            return new PdfPCell(phrase)
            {
                Border = Rectangle.BOX,
                Padding = 6f,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
        }

        private static void AddFooter(Document document, ProposalSummaryViewModel summary)
        {
            var footer = new PdfPTable(2)
            {
                WidthPercentage = 100,
                SpacingBefore = 30f
            };
            footer.SetWidths(new[] { 1f, 1f });

            var stampParagraph = new Paragraph("Kaşe", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11));
            stampParagraph.Add(new Chunk("\n"));
            stampParagraph.Add(new Phrase(summary.CompanyStampText ?? ""));
            var stampCell = new PdfPCell(stampParagraph)
            {
                MinimumHeight = 120f,
                VerticalAlignment = Element.ALIGN_TOP
            };
            footer.AddCell(stampCell);

            var signatureCell = new PdfPCell
            {
                MinimumHeight = 120f,
                VerticalAlignment = Element.ALIGN_TOP
            };
            if (!string.IsNullOrWhiteSpace(summary.AuthorizedSignatureImageUrl))
            {
                try
                {
                    var signature = Image.GetInstance(summary.AuthorizedSignatureImageUrl);
                    signature.ScaleToFit(160f, 80f);
                    signatureCell.AddElement(signature);
                }
                catch
                {
                    signatureCell.AddElement(new Phrase(""));
                }
            }

            var signatureInfo = new Paragraph
            {
                SpacingBefore = 10f
            };
            signatureInfo.Add(new Phrase(summary.AuthorizedSignatureName ?? string.Empty, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)));
            if (!string.IsNullOrWhiteSpace(summary.AuthorizedSignatureTitle))
            {
                signatureInfo.Add(new Chunk("\n"));
                signatureInfo.Add(new Phrase(summary.AuthorizedSignatureTitle, FontFactory.GetFont(FontFactory.HELVETICA, 10)));
            }

            signatureCell.AddElement(signatureInfo);
            footer.AddCell(signatureCell);

            if (!string.IsNullOrWhiteSpace(summary.Notes))
            {
                var note = new Paragraph("Notlar", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11))
                {
                    SpacingBefore = 14f,
                    SpacingAfter = 4f
                };
                note.Add(new Chunk("\n"));
                note.Add(new Phrase(summary.Notes, FontFactory.GetFont(FontFactory.HELVETICA, 10)));
                document.Add(note);
            }

            document.Add(footer);
        }

        private static void AddTableHeader(PdfPTable table, params string[] titles)
        {
            foreach (var title in titles)
            {
                table.AddCell(new PdfPCell(new Phrase(title, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)))
                {
                    BackgroundColor = new BaseColor(240, 240, 240),
                    Padding = 6f,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
            }
        }

        private static PdfPCell CreateTextCell(string text)
        {
            return new PdfPCell(new Phrase(text ?? string.Empty, FontFactory.GetFont(FontFactory.HELVETICA, 9)))
            {
                Padding = 5f,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
        }
    }
}
