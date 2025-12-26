using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using FashionStore.Models;

namespace FashionStore.Areas.Admin.Controllers
{
    public class OrderController : Controller
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        // ----------------------------------------
        // GET: Admin/Order/Index (Danh sách đơn hàng)
        // ----------------------------------------
        public ActionResult Index(string status)
        {
            var orders = db.Orders.OrderByDescending(o => o.CreatedAt).AsQueryable();

            // Lọc theo trạng thái nếu có tham số truyền vào
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            return View(orders.ToList());
        }

        // ----------------------------------------
        // GET: Admin/Order/Details/5 (Chi tiết đơn hàng)
        // ----------------------------------------
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Load đơn hàng kèm theo chi tiết sản phẩm (OrderItems)
            var order = db.Orders
                          .Include(o => o.OrderItems)
                          .SingleOrDefault(o => o.OrderId == id);

            if (order == null) return HttpNotFound();

            return View(order);
        }

        // ----------------------------------------
        // POST: Cập nhật trạng thái đơn hàng
        // ----------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                order.Status = status;
                db.SaveChanges();
                TempData["Message"] = $"Cập nhật đơn hàng {order.OrderCode} thành công!";
            }
            return RedirectToAction("Details", new { id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}