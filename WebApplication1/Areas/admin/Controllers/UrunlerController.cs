using Dapper;  
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;


namespace WebApplication1.Areas.admin.Controllers
{
    [Authorize]
    public class UrunlerController : Controller
    {
		// GET: admin/Urunler
		public ActionResult Index()
		{
			using (var conn = new SqlConnection(
				   ConfigurationManager.ConnectionStrings["KahveSql"].ConnectionString))
			{
				 string sql = @"SELECT * FROM urunler";

				var model = conn.Query<UrunlerViewModel>(sql).ToList();
				return View(model);
			}
		}
		public ActionResult Yeni()
		{
			var model = new UrunlerViewModel();
			return View("UrunForm", model);
		}
		//Fill model yerine kullanabileceğimiz bir yapı 
		public ActionResult Guncelle(int id)
		{
			using (var conn = new SqlConnection(
				  ConfigurationManager.ConnectionStrings["KahveSql"].ConnectionString))
			{
				string sql = @"SELECT * FROM urunler where id=@id";
				var cmd = new SqlCommand(sql, conn);
				cmd.Parameters.AddWithValue("@id", id);
				var model = conn.QueryFirstOrDefault<UrunlerViewModel>(sql, new { id });
				if (model == null)
				{
					return HttpNotFound();
				}

				return View("UrunForm", model);
			}
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)] // icerik HTML ise gerekebilir (AllowHtml zaten var)
		public ActionResult Kaydet(UrunlerViewModel m, HttpPostedFileBase fotoFile)
		{
			// Yeni kayıt ve foto zorunluysa:
			if ((m.id == 0) && (fotoFile == null || fotoFile.ContentLength == 0))
			{
				ViewBag.HataFoto = "Lütfen Fotoğraf Yükleyin!";
				return View("UrunForm", m);
			}

			if (!ModelState.IsValid)
				return View("UrunForm", m);

			// Fotoğraf kaydetme
			if (fotoFile != null && fotoFile.ContentLength > 0)
			{
				var fileName = Path.GetFileName(fotoFile.FileName);
				var folder = Server.MapPath("~/Content/img");
				if (!Directory.Exists(folder)) Directory.CreateDirectory(folder); // klasör yoksa oluştur
				var path = Path.Combine(folder, fileName);
				fotoFile.SaveAs(path);
				m.foto = fileName;
			}

			using (var conn = new SqlConnection(
				ConfigurationManager.ConnectionStrings["KahveSql"].ConnectionString)) // metadata'sız
			{
				if (m.id == 0) // yeni kayıt
				{
					const string insertSql = @"
                INSERT INTO urunler (foto, baslik, ustbaslik, icerik, sira, aktif)
                VALUES (@foto, @baslik, @ustbaslik, @icerik, @sira, @aktif)";

					conn.Execute(insertSql, m);
				}
				else // güncelleme
				{
					const string updateSql = @"
                UPDATE urunler
                SET baslik    = @baslik,
                    ustbaslik = @ustbaslik,
                    icerik    = @icerik,
                    sira      = @sira,
                    aktif     = @aktif,
                    foto      = COALESCE(@foto, foto)
                WHERE id = @id";

					conn.Execute(updateSql, m);
				}
			}

			return RedirectToAction("Index");
		}
	}
}
