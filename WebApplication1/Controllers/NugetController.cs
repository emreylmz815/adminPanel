using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace BusinessCasual4.Controllers
{
    public class NugetController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

		public ActionResult Products()
		{
			using (kahveEntities db = new kahveEntities())
			{
				var model = db.urunler.ToList();
				return View(model);
			}
		}

        [Route("Urun/{id}/{baslik}")]
        public ActionResult UrunDetay(int id)
        {
            using (kahveEntities db = new kahveEntities())
            {
                var model = db.urunler.Where(x => x.id == id).FirstOrDefault();
				if (model == null)
				{
					return HttpNotFound();
				}
				return View(model);
			}
        }


			public ActionResult Store()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}