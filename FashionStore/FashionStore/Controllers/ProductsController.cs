using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FashionStore.Models;

namespace FashionStore.Controllers
{
    public class ProductsController : Controller
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        // ProductsController.cs
        public ActionResult Cat()
        {
            var groups = db.CategoryGroups.Where(g => g.IsActive == true).OrderBy(g => g.SortOrder).ToList();
            return PartialView(groups);
        }
        // GET: Products
        public ActionResult Index(string search, int? catId, decimal? minPrice, decimal? maxPrice)
        {
            var products = db.Products.Where(p => p.IsActive).AsQueryable();

            if (catId.HasValue)
                products = products.Where(p => p.CategoryId == catId);

            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.ProductName.Contains(search));

            if (minPrice.HasValue)
                products = products.Where(p => p.BasePrice >= minPrice.Value);

            if (maxPrice.HasValue)
                products = products.Where(p => p.BasePrice <= maxPrice.Value);

            // QUAN TRỌNG: Tên ViewBag phải là catId để khớp với @Html.DropDownList("catId",...)
            // Và sửa lỗi bool? bằng cách so sánh == true
            var categories = db.Categories.Where(c => c.IsActive == true).ToList();
            ViewBag.catId = new SelectList(categories, "CategoryId", "CatName", catId);

            return View(products.OrderByDescending(p => p.CreatedAt).ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            var product = db.Products.Find(id);
            if (product == null)
                return HttpNotFound();

            var relatedProducts = db.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id && p.IsActive == true)
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;

            var reviews = (
                from r in db.ProductReviews
                join oi in db.OrderItems on r.OrderItemId equals oi.OrderItemId
                join pv in db.ProductVariants on oi.VariantId equals pv.VariantId
                join o in db.Orders on oi.OrderId equals o.OrderId
                where pv.ProductId == id
                orderby r.CreatedAt descending
                select new ReviewViewModel
                {
                    FullName = o.CustomerName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    SizeName = oi.SizeName,
                    ColorName = oi.ColorName
                }
            ).ToList();

            ViewBag.Reviews = reviews;

            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CatSlug");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductId,SKU,ProductName,Slug,Description,BasePrice,IsActive,CreatedAt,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CatSlug", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CatSlug", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductId,SKU,ProductName,Slug,Description,BasePrice,IsActive,CreatedAt,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CatSlug", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
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
