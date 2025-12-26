using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using FashionStore.Areas.Admin.Models;
using FashionStore.Models;
using System.Collections.Generic; // Thêm thư viện này để dùng List

namespace FashionStore.Areas.Admin.Controllers
{
    public class ProductVariantController : Controller
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        // 1. Danh sách các biến thể
        public ActionResult Index(int? productId)
        {
            if (productId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var product = db.Products.Find(productId);
            if (product == null) return HttpNotFound();

            ViewBag.Product = product;

            var variants = db.ProductVariants
                             .Include(v => v.Size)
                             .Include(v => v.Color)
                             .Where(v => v.ProductId == productId)
                             .OrderBy(v => v.Size.SortOrder)
                             .ToList();
            return View(variants);
        }

        // 2. Thêm mới biến thể - GET
        public ActionResult Create(int? productId)
        {
            if (productId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var product = db.Products.Find(productId);
            if (product == null) return HttpNotFound();

            var model = new ProductVariantViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.BasePrice,
                Stock = 0
            };

            // Nạp dữ liệu cho View
            PrepareViewBag(null, null);

            return View(model);
        }

        // 2. Thêm mới biến thể - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductVariantViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool exists = db.ProductVariants.Any(v => v.ProductId == model.ProductId
                                                       && v.SizeId == model.SizeId
                                                       && v.ColorId == model.ColorId);
                if (exists)
                {
                    ModelState.AddModelError("", "Biến thể này (Size + Màu) đã tồn tại!");
                }
                else
                {
                    var variant = new ProductVariant
                    {
                        ProductId = model.ProductId,
                        SizeId = model.SizeId,
                        ColorId = model.ColorId,
                        Price = model.Price,
                        Stock = model.Stock,
                        SKU = $"{model.ProductId}-{model.SizeId}-{model.ColorId}"
                    };

                    db.ProductVariants.Add(variant);
                    db.SaveChanges();
                    return RedirectToAction("Index", new { productId = model.ProductId });
                }
            }

            // QUAN TRỌNG: Nếu có lỗi (trùng biến thể hoặc sai dữ liệu), phải nạp lại ViewBag
            PrepareViewBag(model.SizeId, model.ColorId);
            return View(model);
        }

        // Hàm dùng chung để nạp ViewBag, tránh lặp code và tránh lỗi Null
        private void PrepareViewBag(int? selectedSize, int? selectedColor)
        {
            ViewBag.SizeId = new SelectList(db.Sizes.OrderBy(s => s.SortOrder), "SizeId", "SizeCode", selectedSize);

            // Nạp danh sách Color đầy đủ để View vẽ Radio Button và lấy mã Hex
            ViewBag.ColorsList = db.Colors.ToList();

            // Vẫn giữ ViewBag.ColorId nếu bạn lỡ dùng ở đâu đó khác
            ViewBag.ColorId = new SelectList(db.Colors, "ColorId", "ColorName", selectedColor);
        }

        // 3. Xóa biến thể
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var variant = db.ProductVariants.Find(id);
            if (variant == null) return HttpNotFound();

            int productId = variant.ProductId;
            db.ProductVariants.Remove(variant);
            db.SaveChanges();

            return RedirectToAction("Index", new { productId = productId });
        }
    }
}