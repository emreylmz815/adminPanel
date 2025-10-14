using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class MailSettingsViewModel
    {
        [Display(Name = "Gönderici Adı")]
        public string DisplayName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Gönderici E-posta")]
        public string EmailAddress { get; set; }

        [Required]
        [Display(Name = "SMTP Sunucusu")]
        public string SmtpHost { get; set; }

        [Range(1, 65535)]
        [Display(Name = "SMTP Portu")]
        public int SmtpPort { get; set; } = 587;

        [Display(Name = "SMTP Kullanıcı Adı")]
        public string SmtpUsername { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "SMTP Şifresi")]
        public string SmtpPassword { get; set; }

        [Display(Name = "SMTP SSL Kullanılsın mı?")]
        public bool SmtpUseSsl { get; set; } = true;

        [Display(Name = "IMAP Sunucusu")]
        public string ImapHost { get; set; }

        [Range(1, 65535)]
        [Display(Name = "IMAP Portu")]
        public int ImapPort { get; set; } = 993;

        [Display(Name = "IMAP Kullanıcı Adı")]
        public string ImapUsername { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "IMAP Şifresi")]
        public string ImapPassword { get; set; }

        [Display(Name = "IMAP SSL Kullanılsın mı?")]
        public bool ImapUseSsl { get; set; } = true;

        [Display(Name = "Varsayılan Klasör")]
        public string InboxFolder { get; set; } = "INBOX";
    }
}
