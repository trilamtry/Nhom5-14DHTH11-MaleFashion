using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FashionStore.Models;

namespace FashionStore.Controllers
{
    public class ProductReviewsController : Controller
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        // GET: ProductReviews
        public ActionResult Index()
        {
            var productReviews = db.ProductReviews.Include(p => p.OrderItem);
            return View(productReviews.ToList());
        }

        // GET: ProductReviews/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductReview productReview = db.ProductReviews.Find(id);
            if (productReview == null)
            {
                return HttpNotFound();
            }
            return View(productReview);
        }

        // GET: ProductReviews/Create
        public ActionResult Create(int productId)
        {
            var user = Session["user"] as AppUser;
            if (user == null) return RedirectToAction("Login", "AppUsers");

            // LOGIC MỚI:
            // Tìm OrderItem của sản phẩm này, thuộc User này
            // VÀ ID của OrderItem đó CHƯA TỒN TẠI trong bảng ProductReviews
            var availableItemToReview = (from oi in db.OrderItems
                                         join o in db.Orders on oi.OrderId equals o.OrderId
                                         join pv in db.ProductVariants on oi.VariantId equals pv.VariantId
                                         where pv.ProductId == productId
                                               && o.UserId == user.UserId
                                               // Dòng quan trọng nhất: Loại bỏ các OrderItem đã có trong bảng Review
                                               && !db.ProductReviews.Any(r => r.OrderItemId == oi.OrderItemId)
                                         select oi).FirstOrDefault();

            // Trường hợp 1: Chưa mua bao giờ HOẶC đã mua bao nhiêu cái thì đánh giá hết bấy nhiêu rồi
            if (availableItemToReview == null)
            {
                // Kiểm tra xem họ đã từng mua chưa (để hiển thị thông báo cho đúng)
                bool hasPurchased = (from oi in db.OrderItems
                                     join o in db.Orders on oi.OrderId equals o.OrderId
                                     join pv in db.ProductVariants on oi.VariantId equals pv.VariantId
                                     where pv.ProductId == productId && o.UserId == user.UserId
                                     select oi).Any();

                if (!hasPurchased)
                {
                    TempData["ReviewMessage"] = "Bạn chưa mua sản phẩm này nên không thể đánh giá";
                    TempData["MessageType"] = "danger";
                }
                else
                {
                    TempData["ReviewMessage"] = "Bạn đã đánh giá tất cả các lượt mua của sản phẩm này rồi";
                    TempData["MessageType"] = "warning";
                }

                return RedirectToAction("Details", "Products", new { id = productId });
            }

            // Trường hợp 2: Tìm thấy một đơn hàng chưa đánh giá -> Cho phép đánh giá
            var model = new ProductReview
            {
                OrderItemId = availableItemToReview.OrderItemId, // Gắn ID của đơn hàng mới tìm được
                ReviewCode = "REV-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                Rating = 5
            };

            return View(model);
        }

        // POST: ProductReviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductReview review)
        {
            // KIỂM TRA LẠI: Tránh gửi đánh giá trùng lặp
            var alreadyReviewed = db.ProductReviews.Any(r => r.OrderItemId == review.OrderItemId);
            if (alreadyReviewed)
            {
                ModelState.AddModelError("", "Sản phẩm này đã được đánh giá trước đó.");
                return View(review);
            }

            if (ModelState.IsValid)
            {
                review.CreatedAt = DateTime.Now;
                db.ProductReviews.Add(review);
                db.SaveChanges();

                var productId = (from oi in db.OrderItems
                                 join pv in db.ProductVariants on oi.VariantId equals pv.VariantId
                                 where oi.OrderItemId == review.OrderItemId
                                 select pv.ProductId).FirstOrDefault();

                return RedirectToAction("Details", "Products", new { id = productId });
            }
            TempData["ReviewMessage"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            TempData["MessageType"] = "success";
            return View(review);
        }

        // GET: ProductReviews/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductReview productReview = db.ProductReviews.Find(id);
            if (productReview == null)
            {
                return HttpNotFound();
            }
            ViewBag.OrderItemId = new SelectList(db.OrderItems, "OrderItemId", "ProductName", productReview.OrderItemId);
            return View(productReview);
        }

        // POST: ProductReviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ReviewId,ReviewCode,OrderItemId,Comment,CreatedAt,Rating")] ProductReview productReview)
        {
            if (ModelState.IsValid)
            {
                db.Entry(productReview).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.OrderItemId = new SelectList(db.OrderItems, "OrderItemId", "ProductName", productReview.OrderItemId);
            return View(productReview);
        }

        // GET: ProductReviews/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductReview productReview = db.ProductReviews.Find(id);
            if (productReview == null)
            {
                return HttpNotFound();
            }
            return View(productReview);
        }

        // POST: ProductReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductReview productReview = db.ProductReviews.Find(id);
            db.ProductReviews.Remove(productReview);
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
