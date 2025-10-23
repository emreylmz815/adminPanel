using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WebApplication1.Services.CRM;

namespace WebApplication1.Models.CRM.ViewModels
{
    public class CompanyListViewModel
    {
        public string Search { get; set; }
        public string Tag { get; set; }
        public bool? Active { get; set; }
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 20;
        public PagedResult<Company> Companies { get; set; }
    }

    public class CompanyFormViewModel
    {
        public Company Company { get; set; }
        public string Tags { get; set; }

        public void SyncTagsFromCompany()
        {
            Tags = Company == null ? null : string.Join(", ", Company.TagList);
        }

        public void ApplyTagsToCompany()
        {
            if (Company == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Tags))
            {
                Company.TagList = Array.Empty<string>();
            }
            else
            {
                var split = Tags.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < split.Length; i++)
                {
                    split[i] = split[i].Trim();
                }

                Company.TagList = split;
            }
        }
    }

    public class CompanyBulkTagsViewModel
    {
        [Required]
        public Guid[] CompanyIds { get; set; }

        public string TagsToAdd { get; set; }
        public string TagsToRemove { get; set; }

        public IEnumerable<string> GetTagsToAdd() => Parse(TagsToAdd);
        public IEnumerable<string> GetTagsToRemove() => Parse(TagsToRemove);

        private static IEnumerable<string> Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return Array.Empty<string>();
            }

            return raw
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t));
        }
    }

    public class CompanyDetailsViewModel
    {
        public Company Company { get; set; }
        public IReadOnlyCollection<Contact> Contacts { get; set; }
        public IReadOnlyCollection<CrmNote> Notes { get; set; }
        public ContactFormViewModel ContactForm { get; set; }
        public NoteFormViewModel NoteForm { get; set; }
    }

    public class ContactFormViewModel
    {
        public Guid CompanyId { get; set; }
        public Guid? Id { get; set; }

        [Required]
        [StringLength(120, MinimumLength = 2)]
        public string FullName { get; set; }

        [StringLength(120)]
        public string Title { get; set; }

        [EmailAddress]
        [StringLength(160)]
        public string Email { get; set; }

        [StringLength(32)]
        public string Phone { get; set; }

        [StringLength(4000)]
        public string Notes { get; set; }

        public bool IsPrimary { get; set; }
    }

    public class NoteFormViewModel
    {
        public Guid? CompanyId { get; set; }
        public Guid? ContactId { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 1)]
        public string Content { get; set; }

        public bool Pinned { get; set; }
    }
}
