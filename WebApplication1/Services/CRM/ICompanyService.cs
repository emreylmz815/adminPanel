using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Models.CRM;

namespace WebApplication1.Services.CRM
{
    public interface ICompanyService
    {
        Task<PagedResult<Company>> SearchAsync(string search, string tag, bool? active, int page, int size);
        Task<Company> GetByIdAsync(Guid id);
        Task<Company> CreateAsync(Company company, string userId);
        Task<Company> UpdateAsync(Company company, string userId);
        Task DeactivateAsync(Guid id, string userId);
        Task BulkUpdateTagsAsync(IEnumerable<Guid> companyIds, IEnumerable<string> tagsToAdd, IEnumerable<string> tagsToRemove, string userId);
    }
}
