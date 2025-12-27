using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using FashionStore.Areas.Admin.Models;
using FashionStore.Models;

namespace FashionStore.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        // GET: Product/Index
        public ActionResult Index(string search)
        {
            // 1. Khởi tạo truy vấn dạng IQueryable (Chưa truy vấn vào Database)
            // Việc dùng AsQueryable giúp việc lọc diễn ra tại SQL Server thay vì kéo hết về RAM
            var productsQuery = db.Products
                                 .Include(p => p.Category)
                                 .Include(p => p.ProductVariants)
                                 .Include(p => p.ProductImages)
                                 .AsQueryable();

            // 2. Thực hiện lọc nếu có từ khóa (Lọc tại Database)
            if (!string.IsNullOrEmpty(search))
            {
                string searchLower = search.ToLower();
                productsQuery = productsQuery.Where(p => p.ProductName.ToLower().Contains(searchLower)
                                                      || p.SKU.ToLower().Contains(searchLower));
            }

            // 3. Thực thi truy vấn, sắp xếp và đưa về danh sách List
            var result = productsQuery.OrderByDescending(p => p.CreatedAt).ToList();

            // Trả về kết quả cho View
            return View(result);
        }

        // GET: Product/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                .Include(p => p.ProductImages)
                .SingleOrDefault(p => p.ProductId == id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // ========================== CREATE ==========================

        // GET: Product/Create
        public ActionResult Create()
        {
            // Load danh sách Category để chọn (Single Select)
            ViewBag.Categories = new SelectList(db.Categories, "CategoryId", "CatName");
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Map ViewModel sang Entity Product
                var product = new Product
                {
                    ProductName = model.ProductName,
                    SKU = model.SKU,
                    BasePrice = model.BasePrice,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now,
                    Slug = string.IsNullOrEmpty(model.Slug) ? GenerateSlug(model.ProductName) : model.Slug,
                    CategoryId = model.CategoryId// Gán CategoryId
                };

                db.Products.Add(product);
                db.SaveChanges(); // Lưu để có ProductId trước khi lưu ảnh

                // 2. Xử lý Ảnh đại diện (Nếu có)
                if (model.PrimaryImage != null && model.PrimaryImage.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(model.PrimaryImage.FileName);
                    string extension = Path.GetExtension(fileName);
                    string newFileName = product.Slug + "-thumb" + extension;

                    // Lưu file vào thư mục ~/Content/img/
                    string path = Path.Combine(Server.MapPath("~/assets/img/product"), newFileName);
                    model.PrimaryImage.SaveAs(path);

                    // Lưu vào bảng ProductImage
                    var img = new ProductImage
                    {
                        ProductId = product.ProductId,
                        ImageUrl = newFileName,
                        IsPrimary = true,
                        SortOrder = 1
                    };
                    db.ProductImages.Add(img);
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(db.Categories, "CategoryId", "CategoryName", model.CategoryId);
            return View(model);
        }

        // ========================== EDIT ==========================

        // GET: Product/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Load product kèm Category để fill vào form
            var product = db.Products.Include(p => p.Category).SingleOrDefault(p => p.ProductId == id);
            if (product == null) return HttpNotFound();

            // Map Entity sang ViewModel
            var model = new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                SKU = product.SKU,
                Slug = product.Slug,
                BasePrice = product.BasePrice,
                Description = product.Description,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId
            };

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CatName", model.CategoryId);
            return View(model);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var productToUpdate = db.Products.Include(p => p.Category).SingleOrDefault(p => p.ProductId == model.ProductId);

                if (productToUpdate != null)
                {
                    // Update thông tin cơ bản
                    productToUpdate.ProductName = model.ProductName;
                    productToUpdate.SKU = model.SKU;
                    productToUpdate.BasePrice = model.BasePrice;
                    productToUpdate.Description = model.Description;
                    productToUpdate.IsActive = model.IsActive;
                    productToUpdate.CategoryId = model.CategoryId; // Update CategoryId

                    if (!string.IsNullOrEmpty(model.Slug))
                        productToUpdate.Slug = model.Slug;

                    // Update Ảnh (Nếu user upload ảnh mới)
                    if (model.PrimaryImage != null && model.PrimaryImage.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(model.PrimaryImage.FileName);
                        string extension = Path.GetExtension(fileName);
                        string newFileName = productToUpdate.Slug + "-thumb" + extension;

                        // Lưu file vào thư mục ~/Content/img/
                        string path = Path.Combine(Server.MapPath("~/Content/img/"), newFileName);
                        model.PrimaryImage.SaveAs(path);

                        // Xóa ảnh cũ (nếu có)
                        var oldImage = db.ProductImages.FirstOrDefault(imgs => imgs.ProductId == productToUpdate.ProductId && imgs.IsPrimary);
                        if (oldImage != null)
                        {
                            db.ProductImages.Remove(oldImage);
                        }

                        // Thêm ảnh mới
                        var img = new ProductImage
                        {
                            ProductId = productToUpdate.ProductId,
                            ImageUrl = newFileName,
                            IsPrimary = true,
                            SortOrder = 1
                        };
                        db.ProductImages.Add(img);
                    }

                    db.Entry(productToUpdate).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CatName", model.CategoryId);
            return View(model);
        }

        // ========================== DELETE ==========================

        // GET: Product/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Include(p => p.Category).SingleOrDefault(p => p.ProductId == id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Include(p => p.ProductVariants)
                                         .Include(p => p.ProductImages)
                                         .SingleOrDefault(p => p.ProductId == id);

            if (product != null)
            {
                // Xóa các bảng phụ thuộc trước (nếu không setup Cascade Delete trong SQL)
                db.ProductVariants.RemoveRange(product.ProductVariants);
                db.ProductImages.RemoveRange(product.ProductImages);

                db.Products.Remove(product);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Hàm tiện ích tạo Slug
        public string GenerateSlug(string phrase)
        {
            string str = RemoveAccent(phrase).ToLower();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }

        private string RemoveAccent(string txt)
        {
            byte[] bytes = Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return Encoding.ASCII.GetString(bytes);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}