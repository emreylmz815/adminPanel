using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Models.CRM;

namespace WebApplication1.Services.CRM
{
    public interface ICrmNoteService
    {
        Task<IReadOnlyCollection<CrmNote>> GetForCompanyAsync(Guid companyId);
        Task<IReadOnlyCollection<CrmNote>> GetForContactAsync(Guid contactId);
        Task<CrmNote> GetAsync(Guid id);
        Task<CrmNote> CreateAsync(CrmNote note, string userId);
        Task<CrmNote> UpdateAsync(CrmNote note, string userId);
        Task DeleteAsync(Guid id);
        Task PinAsync(Guid id);
        Task UnpinAsync(Guid id);
    }
}
