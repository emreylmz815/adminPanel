using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WebApplication1.Models
{
	public class UrunlerViewModel
	{
		public int id { get; set; }
		public string foto { get; set; }
		public virtual HttpPostedFileBase fotoFile { get; set; }

		[Required]
		[Display (Name = "Başlık")]
		public string baslik { get; set; }
		[Required]
		[Display(Name = "Üst Başlık")]
		public string ustbaslik { get; set; }
		[Required]
		[AllowHtml]
		[Display(Name = "İçerik")]
		 
		public string icerik { get; set; }
		[Required]
		[Display(Name = "Sıra")]
		public int sira{ get; set; }
		 
		public int aktif{ get; set; }
	}
}