using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class MailMessageViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Gönderici")]
        public string From { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Alıcı")]
        public string To { get; set; }

        [Display(Name = "Konu")]
        public string Subject { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "İçerik")]
        public string Body { get; set; }
    }
}
