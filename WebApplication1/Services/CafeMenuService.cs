using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class CafeMenuService
    {
        private const string SessionKey = "CAFE_MENU_STATE";

        public CafeMenuState GetOrCreateMenu(HttpContextBase context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var menu = context.Session[SessionKey] as CafeMenuState;
            if (menu != null)
            {
                return menu;
            }

            menu = CreateDefaultMenu();
            context.Session[SessionKey] = menu;
            return menu;
        }

        public void SaveMenu(HttpContextBase context, CafeMenuState menu)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Session[SessionKey] = menu;
        }

        public void AddCategory(CafeMenuState menu, string name)
        {
            if (menu == null)
            {
                throw new ArgumentNullException(nameof(menu));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (menu.Categories.Any(category => string.Equals(category.Name, name.Trim(), StringComparison.CurrentCultureIgnoreCase)))
            {
                return;
            }

            menu.Categories.Add(new MenuCategory
            {
                Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.Trim())
            });
        }

        public void RemoveCategory(CafeMenuState menu, Guid categoryId)
        {
            if (menu == null)
            {
                throw new ArgumentNullException(nameof(menu));
            }

            menu.Categories = menu.Categories
                .Where(category => category.Id != categoryId)
                .ToList();
        }

        public void AddProduct(CafeMenuState menu, MenuProductInput input)
        {
            if (menu == null)
            {
                throw new ArgumentNullException(nameof(menu));
            }

            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var category = menu.FindCategory(input.CategoryId);
            if (category == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(input.Title))
            {
                return;
            }

            var product = new MenuProduct
            {
                Title = input.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
                Price = input.Price ?? 0,
                ImageUrl = string.IsNullOrWhiteSpace(input.ImageUrl) ? null : input.ImageUrl.Trim()
            };

            category.Products.Add(product);
        }

        public void RemoveProduct(CafeMenuState menu, Guid productId)
        {
            if (menu == null)
            {
                throw new ArgumentNullException(nameof(menu));
            }

            foreach (var category in menu.Categories)
            {
                category.Products = category.Products
                    .Where(product => product.Id != productId)
                    .ToList();
            }
        }

        private static CafeMenuState CreateDefaultMenu()
        {
            var menu = new CafeMenuState();

            var hotDrinks = new MenuCategory { Name = "Sıcak İçecekler" };
            hotDrinks.Products.AddRange(new List<MenuProduct>
            {
                new MenuProduct
                {
                    Title = "Latte",
                    Description = "Espresso ve buharda ısıtılmış süt ile hazırlanmış klasik lezzet.",
                    Price = 45m
                },
                new MenuProduct
                {
                    Title = "Türk Kahvesi",
                    Description = "Özenle kavrulmuş çekirdeklerden, közde pişirilmiş geleneksel tat.",
                    Price = 30m
                }
            });

            var coldDrinks = new MenuCategory { Name = "Soğuk İçecekler" };
            coldDrinks.Products.AddRange(new List<MenuProduct>
            {
                new MenuProduct
                {
                    Title = "Cold Brew",
                    Description = "12 saat demleme ile yumuşak içimli soğuk kahve deneyimi.",
                    Price = 55m
                },
                new MenuProduct
                {
                    Title = "Limonata",
                    Description = "Taze sıkılmış limon suyu ve nane yapraklarıyla ferahlatıcı.",
                    Price = 35m
                }
            });

            var desserts = new MenuCategory { Name = "Tatlılar" };
            desserts.Products.AddRange(new List<MenuProduct>
            {
                new MenuProduct
                {
                    Title = "San Sebastian Cheesecake",
                    Description = "Karamelize kabuğu ve kremsi dokusuyla favori tatlımız.",
                    Price = 60m
                },
                new MenuProduct
                {
                    Title = "Çikolatalı Browni",
                    Description = "Sıcak servis edilen, içi yoğun çikolatalı browni.",
                    Price = 50m
                }
            });

            menu.Categories.AddRange(new[] { hotDrinks, coldDrinks, desserts });
            return menu;
        }
    }
}
