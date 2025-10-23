using System.Collections.Generic;

namespace WebApplication1.Services.CRM
{
    public class PagedResult<T>
    {
        public IReadOnlyCollection<T> Items { get; }
        public int Page { get; }
        public int Size { get; }
        public int TotalItems { get; }

        public PagedResult(IReadOnlyCollection<T> items, int page, int size, int totalItems)
        {
            Items = items;
            Page = page;
            Size = size;
            TotalItems = totalItems;
        }
    }
}
