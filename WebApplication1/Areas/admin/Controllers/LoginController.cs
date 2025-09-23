using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication1.Models;

namespace WebApplication1.Areas.admin.Controllers
{
	public class LoginController : Controller
	{
		private string connectionString = ConfigurationManager.ConnectionStrings["kahveEntities"].ConnectionString;
		// GET: admin/Login

		public ActionResult Index()
		{
			return View();
		}



		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult GirisYap(KullaniciViewModel kullanici, string ReturnUrl)
		{
			if (!ModelState.IsValid)
				return View("Index", kullanici);

			var connectionString = ConfigurationManager
				.ConnectionStrings["KahveSql"] // <-- metadata’sız olanı kullan
				.ConnectionString;

			bool kullaniciVarMi;

			using (var conn = new SqlConnection(connectionString))
			using (var cmd = new SqlCommand(
				"SELECT COUNT(*) FROM Kullanici WHERE ad = @ad AND sifre = @sifre", conn))
			{
				cmd.Parameters.Add("@ad", SqlDbType.NVarChar, 128).Value = (object)kullanici.ad ?? DBNull.Value;
				cmd.Parameters.Add("@sifre", SqlDbType.NVarChar, 256).Value = (object)kullanici.sifre ?? DBNull.Value;

				conn.Open();
				kullaniciVarMi = (int)cmd.ExecuteScalar() > 0;
			}

			if (kullaniciVarMi)
			{
				// isPersistent: kalıcı cookie istiyorsan, modelde RememberMe ile kontrol et
				FormsAuthentication.SetAuthCookie(kullanici.ad, false);

				// Açık yönlendirme riskini engelle
				if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
					return Redirect(ReturnUrl);

				return RedirectToAction("Index", "Urunler");
			}

			ViewBag.ErrorMessage = "Kullanıcı adı veya şifre hatalı";
			return View("Index", kullanici);
		}

		[Authorize]
		public ActionResult CikisYap()
		{
			FormsAuthentication.SignOut();
			return RedirectToAction("Index");
		}

	}
}