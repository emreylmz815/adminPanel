using System;
using System.Configuration;
using System.Web.Configuration;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IMailSettingsProvider
    {
        MailSettings GetSettings();

        void SaveSettings(MailSettings settings);
    }

    public class WebConfigMailSettingsProvider : IMailSettingsProvider
    {
        private const string Prefix = "Mail.";

        public MailSettings GetSettings()
        {
            var configuration = WebConfigurationManager.OpenWebConfiguration("~");
            var settings = configuration.AppSettings.Settings;

            return new MailSettings
            {
                DisplayName = GetValue(settings, "DisplayName"),
                EmailAddress = GetValue(settings, "EmailAddress"),
                SmtpHost = GetValue(settings, "SmtpHost"),
                SmtpPort = GetIntValue(settings, "SmtpPort", 587),
                SmtpUsername = GetValue(settings, "SmtpUsername"),
                SmtpPassword = GetValue(settings, "SmtpPassword"),
                SmtpUseSsl = GetBoolValue(settings, "SmtpUseSsl", true),
                ImapHost = GetValue(settings, "ImapHost"),
                ImapPort = GetIntValue(settings, "ImapPort", 993),
                ImapUsername = GetValue(settings, "ImapUsername"),
                ImapPassword = GetValue(settings, "ImapPassword"),
                ImapUseSsl = GetBoolValue(settings, "ImapUseSsl", true),
                InboxFolder = GetValue(settings, "InboxFolder") ?? "INBOX"
            };
        }

        public void SaveSettings(MailSettings settings)
        {
            var configuration = WebConfigurationManager.OpenWebConfiguration("~");
            var appSettings = configuration.AppSettings.Settings;

            SetValue(appSettings, "DisplayName", settings.DisplayName);
            SetValue(appSettings, "EmailAddress", settings.EmailAddress);
            SetValue(appSettings, "SmtpHost", settings.SmtpHost);
            SetValue(appSettings, "SmtpPort", settings.SmtpPort.ToString());
            SetValue(appSettings, "SmtpUsername", settings.SmtpUsername);
            SetValue(appSettings, "SmtpPassword", settings.SmtpPassword);
            SetValue(appSettings, "SmtpUseSsl", settings.SmtpUseSsl.ToString());
            SetValue(appSettings, "ImapHost", settings.ImapHost);
            SetValue(appSettings, "ImapPort", settings.ImapPort.ToString());
            SetValue(appSettings, "ImapUsername", settings.ImapUsername);
            SetValue(appSettings, "ImapPassword", settings.ImapPassword);
            SetValue(appSettings, "ImapUseSsl", settings.ImapUseSsl.ToString());
            SetValue(appSettings, "InboxFolder", string.IsNullOrWhiteSpace(settings.InboxFolder) ? "INBOX" : settings.InboxFolder);

            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static string GetValue(KeyValueConfigurationCollection settings, string key)
        {
            return settings[Prefix + key]?.Value;
        }

        private static int GetIntValue(KeyValueConfigurationCollection settings, string key, int defaultValue)
        {
            var value = GetValue(settings, key);
            return int.TryParse(value, out var parsed) ? parsed : defaultValue;
        }

        private static bool GetBoolValue(KeyValueConfigurationCollection settings, string key, bool defaultValue)
        {
            var value = GetValue(settings, key);
            return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
        }

        private static void SetValue(KeyValueConfigurationCollection settings, string key, string value)
        {
            var fullKey = Prefix + key;
            if (settings[fullKey] == null)
            {
                settings.Add(fullKey, value ?? string.Empty);
            }
            else
            {
                settings[fullKey].Value = value ?? string.Empty;
            }
        }
    }
}
