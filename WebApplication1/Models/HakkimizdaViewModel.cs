using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
	public class HakkimizdaViewModel
	{
		public int id { get; set; }
		public string foto { get; set; }
		public virtual HttpPostedFileBase fotoFile { get; set; }
		[Required]
		[Display(Name = "Başlık")]
		public string baslik { get; set; }
		[AllowHtml]
		[Required]
		[Display(Name = "icerik")]
		public string icerik { get; set; }
		[Required]
		[Display(Name = "ustbaslik")]
		public string ustbaslik { get; set; } 
	}
}