namespace WebApplication1.Models
{
    public class MailSettings
    {
        public string DisplayName { get; set; }

        public string EmailAddress { get; set; }

        public string SmtpHost { get; set; }

        public int SmtpPort { get; set; }

        public string SmtpUsername { get; set; }

        public string SmtpPassword { get; set; }

        public bool SmtpUseSsl { get; set; }

        public string ImapHost { get; set; }

        public int ImapPort { get; set; }

        public string ImapUsername { get; set; }

        public string ImapPassword { get; set; }

        public bool ImapUseSsl { get; set; }

        public string InboxFolder { get; set; }
    }
}
