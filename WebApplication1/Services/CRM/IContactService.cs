using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Models.CRM;

namespace WebApplication1.Services.CRM
{
    public interface IContactService
    {
        Task<IReadOnlyCollection<Contact>> GetByCompanyAsync(Guid companyId);
        Task<Contact> GetAsync(Guid id);
        Task<Contact> CreateAsync(Contact contact, string userId);
        Task<Contact> UpdateAsync(Contact contact, string userId);
        Task DeleteAsync(Guid id);
    }
}
