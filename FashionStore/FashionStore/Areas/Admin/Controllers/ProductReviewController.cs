using FashionStore.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace FashionStore.Areas.Admin.Controllers
{
    public class ProductReviewController : Controller
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();
        
        // GET: ProductReview
        // Trang danh sách đánh giá - Lấy tất cả
        public ActionResult Index()
        {
            // Lấy tất cả đánh giá với đầy đủ thông tin
            var reviews = (from r in db.ProductReviews
                           join oi in db.OrderItems on r.OrderItemId equals oi.OrderItemId
                           join o in db.Orders on oi.OrderId equals o.OrderId
                           select new ProductReviewViewModel
                           {
                               ReviewId = r.ReviewId,
                               ReviewCode = r.ReviewCode,
                               Comment = r.Comment,
                               CreatedAt = r.CreatedAt,
                               ProductName = oi.ProductName,
                               SizeName = oi.SizeName,
                               ColorName = oi.ColorName,
                               OrderCode = o.OrderCode,
                               CustomerName = o.CustomerName,
                               CustomerEmail = db.AppUsers
                                   .Where(u => u.UserId == o.UserId)
                                   .Select(u => u.Email)
                                   .FirstOrDefault(),
                               CustomerPhone = o.Phone
                           })
                          .OrderByDescending(r => r.CreatedAt)
                          .ToList();

            // Truyền thống kê
            ViewBag.TotalReviews = db.ProductReviews.Count();
            ViewBag.NewReviews = db.ProductReviews.Count(r => DbFunctions.DiffDays(r.CreatedAt, DateTime.Now) <= 7);
            ViewBag.WithComment = db.ProductReviews.Count(r => !string.IsNullOrEmpty(r.Comment));

            return View(reviews);
        }

        // GET: ProductReview/Details/5
        // Xem chi tiết đánh giá
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var review = (from r in db.ProductReviews
                          join oi in db.OrderItems on r.OrderItemId equals oi.OrderItemId
                          join o in db.Orders on oi.OrderId equals o.OrderId
                          where r.ReviewId == id
                          select new ProductReviewViewModel
                          {
                              ReviewId = r.ReviewId,
                              ReviewCode = r.ReviewCode,
                              Comment = r.Comment,
                              CreatedAt = r.CreatedAt,
                              ProductName = oi.ProductName,
                              SizeName = oi.SizeName,
                              ColorName = oi.ColorName,
                              OrderCode = o.OrderCode,
                              CustomerName = o.CustomerName,
                              CustomerEmail = db.AppUsers
                                  .Where(u => u.UserId == o.UserId)
                                  .Select(u => u.Email)
                                  .FirstOrDefault(),
                              CustomerPhone = o.Phone
                          }).FirstOrDefault();

            if (review == null)
            {
                return HttpNotFound();
            }

            return View(review);
        }

        // POST: ProductReview/SendEmail
        // Gửi email phản hồi cho khách hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendEmail(int reviewId, string emailTo, string emailSubject, string emailBody, bool sendCopy = false)
        {
            try
            {
                // Cấu hình SMTP (cần cấu hình trong Web.config)
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress("noreply@fashionstore.com", "Fashion Store");
                    mail.To.Add(emailTo);
                    mail.Subject = emailSubject;
                    mail.Body = emailBody;
                    mail.IsBodyHtml = false;

                    // Gửi bản sao cho admin nếu được chọn
                    if (sendCopy)
                    {
                        mail.CC.Add("admin@fashionstore.com");
                    }

                    smtpClient.Send(mail);
                }

                TempData["SuccessMessage"] = "Gửi email thành công!";
                return RedirectToAction("Details", new { id = reviewId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi khi gửi email: " + ex.Message;
                return RedirectToAction("Details", new { id = reviewId });
            }
        }

        // GET: ProductReview/Delete/5
        // Xóa đánh giá (nếu cần)
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductReview review = db.ProductReviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }

            return View(review);
        }

        // POST: ProductReview/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductReview review = db.ProductReviews.Find(id);
            db.ProductReviews.Remove(review);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Xóa đánh giá thành công!";
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