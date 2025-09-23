using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
	public class KullaniciViewModel
	{
		[Required(ErrorMessage = "Kullanıcı adı boş bırakılamaz")]
		public string ad { get; set; }

		[Required(ErrorMessage = "Parola boş bırakılamaz")]
		public string sifre { get; set; }

		public virtual bool BeniHatirla { get; set; }
	}
}