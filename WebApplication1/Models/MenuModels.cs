using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebApplication1.Models
{
    public class CafeMenuState
    {
        public CafeMenuState()
        {
            Categories = new List<MenuCategory>();
        }

        public List<MenuCategory> Categories { get; set; }

        public MenuCategory FindCategory(Guid categoryId)
        {
            return Categories.FirstOrDefault(category => category.Id == categoryId);
        }

        public MenuProduct FindProduct(Guid productId)
        {
            foreach (var category in Categories)
            {
                var product = category.Products.FirstOrDefault(item => item.Id == productId);
                if (product != null)
                {
                    return product;
                }
            }

            return null;
        }
    }

    public class MenuCategory
    {
        public MenuCategory()
        {
            Id = Guid.NewGuid();
            Products = new List<MenuProduct>();
        }

        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public List<MenuProduct> Products { get; set; }
    }

    public class MenuProduct
    {
        public MenuProduct()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [StringLength(600)]
        public string Description { get; set; }

        [Url]
        [StringLength(500)]
        public string ImageUrl { get; set; }

        [Range(0, 10000)]
        public decimal Price { get; set; }
    }

    public class MenuCategoryInput
    {
        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kategori adı 100 karakteri geçemez.")]
        public string Name { get; set; }
    }

    public class MenuProductInput
    {
        [Required]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(150, ErrorMessage = "Ürün adı 150 karakteri geçemez.")]
        public string Title { get; set; }

        [StringLength(600, ErrorMessage = "İçerik 600 karakteri geçemez.")]
        public string Description { get; set; }

        [Range(0, 10000, ErrorMessage = "Fiyat 0 ile 10.000 arasında olmalıdır.")]
        public decimal? Price { get; set; }

        [StringLength(500, ErrorMessage = "Görsel bağlantısı 500 karakteri geçemez.")]
        public string ImageUrl { get; set; }
    }

    public class CafeMenuViewModel
    {
        public CafeMenuViewModel()
        {
            CategoryForm = new MenuCategoryInput();
            ProductForm = new MenuProductInput();
        }

        public CafeMenuState Menu { get; set; }

        public Guid ActiveCategoryId { get; set; }

        public MenuCategory ActiveCategory
        {
            get
            {
                return Menu?.FindCategory(ActiveCategoryId);
            }
        }

        public MenuCategoryInput CategoryForm { get; set; }

        public MenuProductInput ProductForm { get; set; }

        public IEnumerable<MenuCategory> OrderedCategories
        {
            get
            {
                return Menu?.Categories
                    .OrderBy(category => category.Name, StringComparer.CurrentCultureIgnoreCase)
                    ?? Enumerable.Empty<MenuCategory>();
            }
        }
    }
}
