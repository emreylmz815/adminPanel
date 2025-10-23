using System;
using System.Threading.Tasks;

namespace WebApplication1.Services.CRM.InMemory
{
    public class InMemoryQuoteNumberGenerator : IQuoteNumberGenerator
    {
        private int _counter = 1;

        public Task<string> GenerateAsync()
        {
            var number = $"QUO-{DateTime.UtcNow:yyyy}-{_counter:0000}";
            _counter++;
            return Task.FromResult(number);
        }
    }
}
