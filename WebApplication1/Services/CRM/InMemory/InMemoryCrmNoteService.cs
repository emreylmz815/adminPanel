using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models.CRM;

namespace WebApplication1.Services.CRM.InMemory
{
    public class InMemoryCrmNoteService : ICrmNoteService
    {
        public InMemoryCrmNoteService()
        {
            InMemoryCrmDataStore.EnsureSeeded();
        }

        public Task<IReadOnlyCollection<CrmNote>> GetByCompanyAsync(Guid companyId)
        {
            var notes = InMemoryCrmDataStore.Notes
                .Where(n => n.CompanyId == companyId)
                .OrderByDescending(n => n.Pinned)
                .ThenByDescending(n => n.CreatedAt)
                .ToList();
            return Task.FromResult((IReadOnlyCollection<CrmNote>)notes);
        }

        public Task<IReadOnlyCollection<CrmNote>> GetByContactAsync(Guid contactId)
        {
            var notes = InMemoryCrmDataStore.Notes
                .Where(n => n.ContactId == contactId)
                .OrderByDescending(n => n.Pinned)
                .ThenByDescending(n => n.CreatedAt)
                .ToList();
            return Task.FromResult((IReadOnlyCollection<CrmNote>)notes);
        }

        public Task<CrmNote> GetByIdAsync(Guid id)
        {
            var note = InMemoryCrmDataStore.Notes.FirstOrDefault(n => n.Id == id);
            return Task.FromResult(note);
        }

        public Task<CrmNote> CreateAsync(CrmNote note, string userId)
        {
            note.Id = Guid.NewGuid();
            note.CreatedAt = DateTime.UtcNow;
            note.CreatedBy = userId;
            InMemoryCrmDataStore.Notes.Add(note);
            return Task.FromResult(note);
        }

        public Task DeleteAsync(Guid id)
        {
            var note = InMemoryCrmDataStore.Notes.FirstOrDefault(n => n.Id == id);
            if (note != null)
            {
                InMemoryCrmDataStore.Notes.Remove(note);
            }

            return Task.CompletedTask;
        }

        public Task PinAsync(Guid id, bool pinned)
        {
            var note = InMemoryCrmDataStore.Notes.FirstOrDefault(n => n.Id == id);
            if (note == null)
            {
                throw new InvalidOperationException("Note not found");
            }

            note.Pinned = pinned;
            return Task.CompletedTask;
        }
    }
}
