using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models.CRM;

namespace WebApplication1.Services.CRM.InMemory
{
    public class InMemoryContactService : IContactService
    {
        public InMemoryContactService()
        {
            InMemoryCrmDataStore.EnsureSeeded();
        }

        public Task<IReadOnlyCollection<Contact>> GetByCompanyAsync(Guid companyId)
        {
            var contacts = InMemoryCrmDataStore.Contacts
                .Where(c => c.CompanyId == companyId)
                .OrderByDescending(c => c.IsPrimary)
                .ThenBy(c => c.FullName)
                .ToList();
            return Task.FromResult((IReadOnlyCollection<Contact>)contacts);
        }

        public Task<Contact> GetByIdAsync(Guid id)
        {
            var contact = InMemoryCrmDataStore.Contacts.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(contact);
        }

        public Task<Contact> CreateAsync(Contact contact, string userId)
        {
            contact.Id = Guid.NewGuid();
            contact.CreatedAt = DateTime.UtcNow;
            contact.CreatedBy = userId;
            InMemoryCrmDataStore.Contacts.Add(contact);
            return Task.FromResult(contact);
        }

        public Task<Contact> UpdateAsync(Contact contact)
        {
            var existing = InMemoryCrmDataStore.Contacts.FirstOrDefault(c => c.Id == contact.Id);
            if (existing == null)
            {
                throw new InvalidOperationException("Contact not found");
            }

            existing.FullName = contact.FullName;
            existing.Title = contact.Title;
            existing.Email = contact.Email;
            existing.Phone = contact.Phone;
            existing.Notes = contact.Notes;
            existing.IsPrimary = contact.IsPrimary;

            if (contact.IsPrimary)
            {
                foreach (var other in InMemoryCrmDataStore.Contacts.Where(c => c.CompanyId == existing.CompanyId && c.Id != existing.Id))
                {
                    other.IsPrimary = false;
                }
            }

            return Task.FromResult(existing);
        }

        public Task DeleteAsync(Guid id)
        {
            var existing = InMemoryCrmDataStore.Contacts.FirstOrDefault(c => c.Id == id);
            if (existing != null)
            {
                InMemoryCrmDataStore.Contacts.Remove(existing);
            }

            return Task.CompletedTask;
        }
    }
}
