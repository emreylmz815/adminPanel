using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication1.Models;

namespace WebApplication1.Areas.admin.Controllers
{
    [Authorize]
    public class HakkimizdaController : Controller
    {
		public ActionResult Index()
		{
			HakkimizdaViewModel model = new HakkimizdaViewModel();
			 
			  string connectionString = ConfigurationManager.ConnectionStrings["KahveSql"].ConnectionString;
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				conn.Open();
				string query = "SELECT TOP 1 * FROM Hakkimizda"; // İlk kaydı çekmek için

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						if (reader.Read()) // Eğer veri varsa
						{
							model.id = Convert.ToInt32(reader["id"]);
							model.foto = reader["foto"].ToString();
							model.baslik = reader["baslik"].ToString();
							model.icerik = reader["icerik"].ToString();
							model.ustbaslik = reader["ustbaslik"].ToString();
						}
					}
				}
			}

			return View(model);
		}

		public ActionResult HakkimizdaGuncelle()
		{
			HakkimizdaViewModel model = new HakkimizdaViewModel();
			string connectionString = ConfigurationManager.ConnectionStrings["kahveEntities"].ConnectionString;
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				conn.Open();
				string query = "SELECT TOP 1 * FROM Hakkimizda"; // İlk kaydı çekmek için

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						if (reader.Read()) // Eğer veri varsa
						{
							model.id = Convert.ToInt32(reader["id"]);
							model.foto = reader["foto"].ToString();
							model.baslik = reader["baslik"].ToString();
							model.icerik = reader["icerik"].ToString();
							model.ustbaslik = reader["ustbaslik"].ToString();
						}
					}
				}
			}
			return View(model);
		}

	}
}
 