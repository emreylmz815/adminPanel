using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models.CRM;

namespace WebApplication1.Services.CRM.InMemory
{
    public class InMemoryCompanyService : ICompanyService
    {
        public InMemoryCompanyService()
        {
            InMemoryCrmDataStore.EnsureSeeded();
        }

        public Task<PagedResult<Company>> SearchAsync(string search, string tag, bool? active, int page, int size)
        {
            var query = InMemoryCrmDataStore.Companies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                    || (!string.IsNullOrEmpty(c.Email) && c.Email.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (!string.IsNullOrEmpty(c.Phone) && c.Phone.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(c => c.TagList.Contains(tag, StringComparer.OrdinalIgnoreCase));
            }

            if (active.HasValue)
            {
                query = query.Where(c => c.IsActive == active.Value);
            }

            var total = query.Count();
            var items = query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();

            return Task.FromResult(new PagedResult<Company>(items, page, size, total));
        }

        public Task<Company> GetByIdAsync(Guid id)
        {
            var company = InMemoryCrmDataStore.Companies.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(company);
        }

        public Task<Company> CreateAsync(Company company, string userId)
        {
            company.Id = Guid.NewGuid();
            company.CreatedAt = DateTime.UtcNow;
            company.CreatedBy = userId;
            InMemoryCrmDataStore.Companies.Add(company);
            return Task.FromResult(company);
        }

        public Task<Company> UpdateAsync(Company company, string userId)
        {
            var existing = InMemoryCrmDataStore.Companies.FirstOrDefault(c => c.Id == company.Id);
            if (existing == null)
            {
                throw new InvalidOperationException("Company not found");
            }

            existing.Name = company.Name;
            existing.TaxNo = company.TaxNo;
            existing.Sector = company.Sector;
            existing.Phone = company.Phone;
            existing.Email = company.Email;
            existing.Website = company.Website;
            existing.Address = company.Address;
            existing.TagList = company.TagList;
            existing.IsActive = company.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = userId;

            return Task.FromResult(existing);
        }

        public Task DeactivateAsync(Guid id, string userId)
        {
            var existing = InMemoryCrmDataStore.Companies.FirstOrDefault(c => c.Id == id);
            if (existing == null)
            {
                throw new InvalidOperationException("Company not found");
            }

            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = userId;
            return Task.CompletedTask;
        }

        public Task BulkUpdateTagsAsync(IEnumerable<Guid> companyIds, IEnumerable<string> tagsToAdd, IEnumerable<string> tagsToRemove, string userId)
        {
            var add = tagsToAdd?.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? Array.Empty<string>();
            var remove = tagsToRemove?.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? Array.Empty<string>();

            foreach (var company in InMemoryCrmDataStore.Companies.Where(c => companyIds.Contains(c.Id)))
            {
                var tags = company.TagList.ToList();
                foreach (var tag in add)
                {
                    if (!tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                    {
                        tags.Add(tag);
                    }
                }

                foreach (var tag in remove)
                {
                    tags.RemoveAll(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
                }

                company.TagList = tags.ToArray();
                company.UpdatedAt = DateTime.UtcNow;
                company.UpdatedBy = userId;
            }

            return Task.CompletedTask;
        }
    }
}
