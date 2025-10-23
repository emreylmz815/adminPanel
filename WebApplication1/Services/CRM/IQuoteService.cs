using System;
using System.Threading.Tasks;
using WebApplication1.Models.CRM;

namespace WebApplication1.Services.CRM
{
    public interface IQuoteService
    {
        Task<PagedResult<Quote>> SearchAsync(QuoteStatus? status, Guid? companyId, string search, int page, int size);
        Task<Quote> GetAsync(Guid id);
        Task<Quote> CreateAsync(Quote quote, string userId);
        Task<Quote> UpdateAsync(Quote quote, string userId);
        Task ChangeStatusAsync(Guid id, QuoteStatus status, string userId);
        Task<byte[]> GeneratePdfAsync(Guid id);
        Task SendEmailAsync(Guid id, string to, string cc, string subject, string body, string userId);
    }

    public interface IQuoteNumberGenerator
    {
        Task<string> GenerateAsync();
    }
}
