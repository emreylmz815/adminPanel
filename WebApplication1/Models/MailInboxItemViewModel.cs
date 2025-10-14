using System;

namespace WebApplication1.Models
{
    public class MailInboxItemViewModel
    {
        public string Subject { get; set; }

        public string From { get; set; }

        public DateTimeOffset Date { get; set; }

        public string Preview { get; set; }
    }
}
