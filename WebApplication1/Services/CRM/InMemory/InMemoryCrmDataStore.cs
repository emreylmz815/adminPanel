using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.Models.CRM;

namespace WebApplication1.Services.CRM.InMemory
{
    /// <summary>
    /// Simple in-memory storage emulating persistence for the CRM module.
    /// </summary>
    public static class InMemoryCrmDataStore
    {
        private static readonly object _sync = new object();
        private static bool _seeded;

        public static IList<Company> Companies { get; } = new List<Company>();
        public static IList<Contact> Contacts { get; } = new List<Contact>();
        public static IList<CrmNote> Notes { get; } = new List<CrmNote>();
        public static IList<Quote> Quotes { get; } = new List<Quote>();
        public static IList<TaskItem> Tasks { get; } = new List<TaskItem>();
        public static IList<TaskComment> TaskComments { get; } = new List<TaskComment>();
        public static IList<PaymentPlan> PaymentPlans { get; } = new List<PaymentPlan>();
        public static IList<PaymentReceipt> PaymentReceipts { get; } = new List<PaymentReceipt>();
        public static IList<KanbanColumn> KanbanColumns { get; } = new List<KanbanColumn>();

        /// <summary>
        /// Ensures the seed data is present. The method is idempotent and thread-safe.
        /// </summary>
        public static void EnsureSeeded()
        {
            if (_seeded)
            {
                return;
            }

            lock (_sync)
            {
                if (_seeded)
                {
                    return;
                }

                SeedCompanies();
                SeedQuotes();
                SeedTasks();
                SeedKanbanColumns();
                SeedPaymentPlans();
                _seeded = true;
            }
        }

        private static void SeedCompanies()
        {
            if (Companies.Count > 0)
            {
                return;
            }

            var now = DateTime.UtcNow;
            for (int i = 1; i <= 5; i++)
            {
                var company = new Company
                {
                    Name = $"Demo Company {i}",
                    TaxNo = $"12345678{i:00}",
                    Sector = i % 2 == 0 ? "Technology" : "Manufacturing",
                    Phone = $"+90 212 555 00{i:00}",
                    Email = $"info{i}@demo.local",
                    Website = "https://demo.local",
                    Address = "Istanbul",
                    TagList = i % 2 == 0
                        ? new[] { "priority", "partner" }
                        : new[] { "prospect" },
                    CreatedAt = now.AddDays(-i),
                    CreatedBy = "seed"
                };
                Companies.Add(company);

                var primaryContact = new Contact
                {
                    CompanyId = company.Id,
                    FullName = $"Primary Contact {i}",
                    Title = "Manager",
                    Email = $"contact{i}@demo.local",
                    Phone = "+90 212 555 0010",
                    IsPrimary = true,
                    CreatedAt = now.AddDays(-i),
                    CreatedBy = "seed"
                };
                Contacts.Add(primaryContact);

                for (int j = 1; j <= 1; j++)
                {
                    Contacts.Add(new Contact
                    {
                        CompanyId = company.Id,
                        FullName = $"Support Contact {i}-{j}",
                        Title = "Specialist",
                        Email = $"support{i}{j}@demo.local",
                        Phone = "+90 212 555 0020",
                        CreatedAt = now.AddDays(-i),
                        CreatedBy = "seed"
                    });
                }

                Notes.Add(new CrmNote
                {
                    CompanyId = company.Id,
                    Content = $"Initial note for {company.Name}",
                    Pinned = i % 2 == 0,
                    CreatedAt = now.AddDays(-i),
                    CreatedBy = "seed"
                });

                Notes.Add(new CrmNote
                {
                    CompanyId = company.Id,
                    Content = "Follow-up scheduled for next week.",
                    CreatedAt = now.AddDays(-(i + 1)),
                    CreatedBy = "seed"
                });
            }
        }

        private static void SeedQuotes()
        {
            if (Quotes.Count > 0)
            {
                return;
            }

            var companies = Companies.Take(3).ToArray();
            for (int i = 0; i < companies.Length; i++)
            {
                var quote = new Quote
                {
                    CompanyId = companies[i].Id,
                    No = $"QUO-2024-00{i + 1}",
                    Title = $"Consulting Package {i + 1}",
                    TaxRate = 18,
                    DiscountRate = 5,
                    CostTotal = 1500 + i * 250,
                    Status = QuoteStatus.Draft,
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    CreatedBy = "seed"
                };

                quote.Lines.Add(new QuoteLine
                {
                    Name = "Implementation",
                    Qty = 1,
                    UnitPrice = 2500,
                    LineCost = 1500
                });

                quote.Lines.Add(new QuoteLine
                {
                    Name = "Training",
                    Qty = 2,
                    UnitPrice = 750,
                    LineCost = 500
                });

                quote.RecalculateTotals();
                Quotes.Add(quote);
            }
        }

        private static void SeedTasks()
        {
            if (Tasks.Count > 0)
            {
                return;
            }

            foreach (var company in Companies.Take(3))
            {
                Tasks.Add(new TaskItem
                {
                    Title = $"Kick-off meeting for {company.Name}",
                    CompanyId = company.Id,
                    Status = TaskStatus.Todo,
                    Priority = TaskPriority.High,
                    DueDate = DateTime.UtcNow.AddDays(3),
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    CreatedBy = "seed"
                });

                Tasks.Add(new TaskItem
                {
                    Title = "Prepare proposal",
                    CompanyId = company.Id,
                    Status = TaskStatus.InProgress,
                    Priority = TaskPriority.Medium,
                    DueDate = DateTime.UtcNow.AddDays(5),
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    CreatedBy = "seed"
                });
            }
        }

        private static void SeedKanbanColumns()
        {
            if (KanbanColumns.Count > 0)
            {
                return;
            }

            KanbanColumns.Add(new KanbanColumn { Name = "Todo", DisplayOrder = 1 });
            KanbanColumns.Add(new KanbanColumn { Name = "InProgress", DisplayOrder = 2 });
            KanbanColumns.Add(new KanbanColumn { Name = "Waiting", DisplayOrder = 3 });
            KanbanColumns.Add(new KanbanColumn { Name = "Done", DisplayOrder = 4 });
        }

        private static void SeedPaymentPlans()
        {
            if (PaymentPlans.Count > 0)
            {
                return;
            }

            var quote = Quotes.FirstOrDefault();
            if (quote == null)
            {
                return;
            }

            var plan = new PaymentPlan
            {
                QuoteId = quote.Id,
                DueDate = DateTime.UtcNow.AddDays(10).Date,
                Amount = 1500,
                Currency = "TRY",
                Status = PaymentStatus.Planned,
                Notes = "Initial payment",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };
            PaymentPlans.Add(plan);

            PaymentPlans.Add(new PaymentPlan
            {
                QuoteId = quote.Id,
                DueDate = DateTime.UtcNow.AddDays(30).Date,
                Amount = 2500,
                Currency = "TRY",
                Status = PaymentStatus.Planned,
                Notes = "Final payment",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            });
        }
    }
}
