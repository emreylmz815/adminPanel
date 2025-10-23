using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Models.CRM;

namespace WebApplication1.Services.CRM.InMemory
{
    public class InMemoryQuoteService : IQuoteService
    {
        private readonly IQuoteNumberGenerator _numberGenerator;

        public InMemoryQuoteService(IQuoteNumberGenerator numberGenerator)
        {
            _numberGenerator = numberGenerator;
            InMemoryCrmDataStore.EnsureSeeded();
        }

        public Task<PagedResult<Quote>> SearchAsync(QuoteStatus? status, Guid? companyId, string search, int page, int size)
        {
            var query = InMemoryCrmDataStore.Quotes.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(q => q.Status == status.Value);
            }

            if (companyId.HasValue)
            {
                query = query.Where(q => q.CompanyId == companyId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(q => q.Title.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                    || q.No.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var total = query.Count();
            var items = query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();

            return Task.FromResult(new PagedResult<Quote>(items, page, size, total));
        }

        public Task<Quote> GetAsync(Guid id)
        {
            var quote = InMemoryCrmDataStore.Quotes.FirstOrDefault(q => q.Id == id);
            if (quote != null)
            {
                quote.RecalculateTotals();
            }

            return Task.FromResult(quote);
        }

        public async Task<Quote> CreateAsync(Quote quote, string userId)
        {
            quote.Id = Guid.NewGuid();
            quote.No = await _numberGenerator.GenerateAsync();
            quote.CreatedAt = DateTime.UtcNow;
            quote.CreatedBy = userId;
            foreach (var line in quote.Lines)
            {
                line.Id = Guid.NewGuid();
                line.QuoteId = quote.Id;
                line.Recalculate();
            }

            quote.RecalculateTotals();
            InMemoryCrmDataStore.Quotes.Add(quote);
            return quote;
        }

        public Task<Quote> UpdateAsync(Quote quote, string userId)
        {
            var existing = InMemoryCrmDataStore.Quotes.FirstOrDefault(q => q.Id == quote.Id);
            if (existing == null)
            {
                throw new InvalidOperationException("Quote not found");
            }

            existing.Title = quote.Title;
            existing.CompanyId = quote.CompanyId;
            existing.TaxRate = quote.TaxRate;
            existing.DiscountRate = quote.DiscountRate;
            existing.CostTotal = quote.CostTotal;
            existing.Notes = quote.Notes;
            existing.ExpiresAt = quote.ExpiresAt;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = userId;

            existing.Lines.Clear();
            foreach (var line in quote.Lines)
            {
                line.Id = line.Id == Guid.Empty ? Guid.NewGuid() : line.Id;
                line.QuoteId = existing.Id;
                line.Recalculate();
                existing.Lines.Add(line);
            }

            existing.RecalculateTotals();
            return Task.FromResult(existing);
        }

        public Task ChangeStatusAsync(Guid id, QuoteStatus status, string userId)
        {
            var quote = InMemoryCrmDataStore.Quotes.FirstOrDefault(q => q.Id == id);
            if (quote == null)
            {
                throw new InvalidOperationException("Quote not found");
            }

            if (!IsValidTransition(quote.Status, status))
            {
                throw new InvalidOperationException($"Cannot transition from {quote.Status} to {status}");
            }

            quote.Status = status;
            quote.UpdatedAt = DateTime.UtcNow;
            quote.UpdatedBy = userId;

            switch (status)
            {
                case QuoteStatus.Sent:
                    quote.SentAt = DateTime.UtcNow;
                    break;
                case QuoteStatus.Approved:
                    quote.ApprovedAt = DateTime.UtcNow;
                    break;
                case QuoteStatus.Rejected:
                    quote.RejectedAt = DateTime.UtcNow;
                    break;
                case QuoteStatus.Expired:
                    quote.ExpiresAt = quote.ExpiresAt ?? DateTime.UtcNow;
                    break;
            }

            return Task.CompletedTask;
        }

        public Task<byte[]> GeneratePdfAsync(Guid id)
        {
            var quote = InMemoryCrmDataStore.Quotes.FirstOrDefault(q => q.Id == id);
            if (quote == null)
            {
                throw new InvalidOperationException("Quote not found");
            }

            quote.RecalculateTotals();
            var builder = new StringBuilder();
            builder.AppendLine($"Quote No: {quote.No}");
            builder.AppendLine($"Title: {quote.Title}");
            builder.AppendLine($"Status: {quote.Status}");
            builder.AppendLine($"Subtotal: {quote.Subtotal:C2}");
            builder.AppendLine($"Tax: {quote.TaxAmount:C2}");
            builder.AppendLine($"Total: {quote.Total:C2}");
            foreach (var line in quote.Lines)
            {
                builder.AppendLine($"- {line.Name} x{line.Qty} @ {line.UnitPrice:C2} = {line.LineTotal:C2}");
            }

            return Task.FromResult(Encoding.UTF8.GetBytes(builder.ToString()));
        }

        public Task SendEmailAsync(Guid id, string to, string cc, string subject, string body, string userId)
        {
            var quote = InMemoryCrmDataStore.Quotes.FirstOrDefault(q => q.Id == id);
            if (quote == null)
            {
                throw new InvalidOperationException("Quote not found");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Email sent to {to}");
            if (!string.IsNullOrWhiteSpace(cc))
            {
                sb.AppendLine($"CC: {cc}");
            }

            sb.AppendLine($"Subject: {subject}");
            sb.AppendLine($"Body: {body}");
            sb.AppendLine($"SentAt: {DateTime.UtcNow:u} by {userId}");

            quote.Notes = string.IsNullOrWhiteSpace(quote.Notes)
                ? sb.ToString()
                : quote.Notes + Environment.NewLine + sb;

            if (quote.Status == QuoteStatus.Draft)
            {
                quote.Status = QuoteStatus.Sent;
                quote.SentAt = DateTime.UtcNow;
            }

            return Task.CompletedTask;
        }

        private static bool IsValidTransition(QuoteStatus from, QuoteStatus to)
        {
            return from switch
            {
                QuoteStatus.Draft => to == QuoteStatus.Sent || to == QuoteStatus.Approved || to == QuoteStatus.Rejected,
                QuoteStatus.Sent => to == QuoteStatus.Approved || to == QuoteStatus.Rejected || to == QuoteStatus.Expired,
                QuoteStatus.Approved => to == QuoteStatus.Approved,
                QuoteStatus.Rejected => to == QuoteStatus.Rejected,
                QuoteStatus.Expired => to == QuoteStatus.Expired,
                _ => false
            };
        }
    }
}
