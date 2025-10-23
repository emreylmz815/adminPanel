using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Services.CRM;

namespace WebApplication1.Models.CRM.ViewModels
{
    public class QuoteListViewModel
    {
        public QuoteStatus? Status { get; set; }
        public Guid? CompanyId { get; set; }
        public string Search { get; set; }
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 20;
        public PagedResult<Quote> Quotes { get; set; }
        public IReadOnlyCollection<Company> Companies { get; set; }
    }

    public class QuoteFormViewModel
    {
        public Quote Quote { get; set; }
        public IReadOnlyCollection<Company> Companies { get; set; }
    }

    public class QuoteStatusChangeViewModel
    {
        public Guid Id { get; set; }
        public string Action { get; set; }
    }

    public class QuoteEmailViewModel
    {
        public Guid QuoteId { get; set; }

        [Required]
        [EmailAddress]
        public string To { get; set; }

        [EmailAddress]
        public string Cc { get; set; }

        [StringLength(160)]
        public string Subject { get; set; }

        [StringLength(4000)]
        public string Body { get; set; }
    }
}
