using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace BusinessCasual4.Controllers
{
    public class MailController : Controller
    {
        private readonly IMailSettingsProvider _settingsProvider;
        private readonly MailService _mailService;

        public MailController()
        {
            _settingsProvider = new WebConfigMailSettingsProvider();
            _mailService = new MailService(_settingsProvider);
        }

        [HttpGet]
        public ActionResult Settings()
        {
            var settings = _settingsProvider.GetSettings();
            var model = new MailSettingsViewModel
            {
                DisplayName = settings.DisplayName,
                EmailAddress = settings.EmailAddress,
                SmtpHost = settings.SmtpHost,
                SmtpPort = settings.SmtpPort,
                SmtpUsername = settings.SmtpUsername,
                SmtpPassword = settings.SmtpPassword,
                SmtpUseSsl = settings.SmtpUseSsl,
                ImapHost = settings.ImapHost,
                ImapPort = settings.ImapPort,
                ImapUsername = settings.ImapUsername,
                ImapPassword = settings.ImapPassword,
                ImapUseSsl = settings.ImapUseSsl,
                InboxFolder = settings.InboxFolder
            };

            if (TempData.ContainsKey("SettingsSaved"))
            {
                ViewBag.SettingsSaved = true;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Settings(MailSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var settings = new MailSettings
            {
                DisplayName = model.DisplayName,
                EmailAddress = model.EmailAddress,
                SmtpHost = model.SmtpHost,
                SmtpPort = model.SmtpPort,
                SmtpUsername = model.SmtpUsername,
                SmtpPassword = model.SmtpPassword,
                SmtpUseSsl = model.SmtpUseSsl,
                ImapHost = model.ImapHost,
                ImapPort = model.ImapPort,
                ImapUsername = model.ImapUsername,
                ImapPassword = model.ImapPassword,
                ImapUseSsl = model.ImapUseSsl,
                InboxFolder = model.InboxFolder
            };

            _settingsProvider.SaveSettings(settings);
            TempData["SettingsSaved"] = true;
            return RedirectToAction("Settings");
        }

        [HttpGet]
        public ActionResult Send()
        {
            var settings = _settingsProvider.GetSettings();
            var model = new MailMessageViewModel
            {
                From = settings.EmailAddress
            };

            if (TempData.ContainsKey("MessageSent"))
            {
                ViewBag.MessageSent = true;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Send(MailMessageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _mailService.SendAsync(model);
                TempData["MessageSent"] = true;
                return RedirectToAction("Send");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Mail gönderilirken hata oluştu: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Inbox()
        {
            try
            {
                var messages = await _mailService.GetInboxAsync(25);
                return View(messages);
            }
            catch (Exception ex)
            {
                ViewBag.InboxError = ex.Message;
                return View(new List<MailInboxItemViewModel>());
            }
        }
    }
}
