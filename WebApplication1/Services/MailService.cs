using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class MailService
    {
        private readonly IMailSettingsProvider _settingsProvider;

        public MailService(IMailSettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task SendAsync(MailMessageViewModel model)
        {
            var settings = _settingsProvider.GetSettings();
            if (string.IsNullOrWhiteSpace(settings.SmtpHost))
            {
                throw new InvalidOperationException("SMTP ayarları eksik.");
            }

            var message = new MimeMessage();
            var fromAddress = string.IsNullOrWhiteSpace(settings.EmailAddress) ? model.From : settings.EmailAddress;
            message.From.Add(new MailboxAddress(settings.DisplayName ?? fromAddress, fromAddress));
            if (!string.Equals(fromAddress, model.From, StringComparison.OrdinalIgnoreCase))
            {
                message.ReplyTo.Add(MailboxAddress.Parse(model.From));
            }
            message.To.Add(MailboxAddress.Parse(model.To));
            message.Subject = model.Subject ?? string.Empty;

            var builder = new BodyBuilder { TextBody = model.Body };
            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, GetSecureSocketOptions(settings.SmtpUseSsl));

                var smtpUsername = string.IsNullOrWhiteSpace(settings.SmtpUsername) ? settings.EmailAddress : settings.SmtpUsername;
                if (!string.IsNullOrWhiteSpace(smtpUsername))
                {
                    await client.AuthenticateAsync(smtpUsername, settings.SmtpPassword);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        public async Task<IList<MailInboxItemViewModel>> GetInboxAsync(int maxCount)
        {
            var settings = _settingsProvider.GetSettings();
            if (string.IsNullOrWhiteSpace(settings.ImapHost))
            {
                throw new InvalidOperationException("IMAP ayarları eksik.");
            }

            using (var client = new ImapClient())
            {
                await client.ConnectAsync(settings.ImapHost, settings.ImapPort, GetSecureSocketOptions(settings.ImapUseSsl));
                var imapUsername = string.IsNullOrWhiteSpace(settings.ImapUsername) ? settings.EmailAddress : settings.ImapUsername;
                await client.AuthenticateAsync(imapUsername, settings.ImapPassword);

                var folderName = string.IsNullOrWhiteSpace(settings.InboxFolder) ? "INBOX" : settings.InboxFolder;
                var inbox = await client.GetFolderAsync(folderName);
                await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);

                var count = inbox.Count;
                if (count == 0)
                {
                    await client.DisconnectAsync(true);
                    return new List<MailInboxItemViewModel>();
                }

                var startIndex = Math.Max(0, count - maxCount);
                var summaries = await inbox.FetchAsync(startIndex, count - 1, MailKit.MessageSummaryItems.Envelope | MailKit.MessageSummaryItems.UniqueId);

                var result = new List<MailInboxItemViewModel>();
                foreach (var summary in summaries.Reverse())
                {
                    var previewText = string.Empty;
                    try
                    {
                        var message = await inbox.GetMessageAsync(summary.UniqueId);
                        if (message != null)
                        {
                            var textPart = message.TextBody ?? message.HtmlBody;
                            if (!string.IsNullOrWhiteSpace(textPart))
                            {
                                previewText = textPart.Trim();
                                if (previewText.Length > 200)
                                {
                                    previewText = previewText.Substring(0, 200) + "...";
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Yoksay
                    }

                    result.Add(new MailInboxItemViewModel
                    {
                        Subject = summary.Envelope?.Subject,
                        From = summary.Envelope?.From?.FirstOrDefault()?.ToString(),
                        Date = summary.Envelope?.Date ?? DateTimeOffset.MinValue,
                        Preview = previewText
                    });
                }

                await client.DisconnectAsync(true);

                return result;
            }
        }

        private static SecureSocketOptions GetSecureSocketOptions(bool useSsl)
        {
            return useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
        }
    }
}
