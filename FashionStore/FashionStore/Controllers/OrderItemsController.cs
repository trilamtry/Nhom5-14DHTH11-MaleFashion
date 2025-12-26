using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FashionStore.Models;
using System.Transactions; // Để đảm bảo tính toàn vẹn dữ liệu

namespace FashionStore.Controllers
{
    public class OrderItemsController : Controller
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();
        // GET: Hiển thị trang thanh toán
        [HttpGet]
        public ActionResult Checkout()
        {
            // 1. Kiểm tra Session đăng nhập
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "AppUsers");
            }

            var user = (AppUser)Session["user"];

            // 2. Lấy giỏ hàng của User
            var cart = db.Carts.Include("CartItems").FirstOrDefault(c => c.UserId == user.UserId);
            if (cart == null || !cart.CartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // 3. Lấy địa chỉ của User (Lấy cái mới nhất)
            var address = db.CustomerAddresses
                            .Where(a => a.UserId == user.UserId)
                            .OrderByDescending(a => a.CreatedAt)
                            .FirstOrDefault();

            // Đưa dữ liệu ra View
            ViewBag.Cart = cart;
            ViewBag.Address = address;
            ViewBag.TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.UnitPrice);

            return View(user);
        }
        // POST: Xử lý lưu đơn hàng[HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessCheckout(string customerName, string phone, string fullAddress, string messageCard)
        {
            if (Session["user"] == null) return RedirectToAction("Login", "Account");
            var user = (AppUser)Session["user"];

            var cart = db.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.UserId == user.UserId);
            if (cart == null || !cart.CartItems.Any()) return RedirectToAction("Index", "Home");

            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    // BƯỚC 1: TẠO ĐƠN HÀNG (ORDER)
                    var newOrder = new Order
                    {
                        OrderCode = "ORD-" + DateTime.Now.Ticks.ToString().Substring(10),
                        UserId = user.UserId,
                        CustomerName = customerName,
                        Phone = phone,
                        AddressLine = fullAddress,
                        MessageCard = messageCard,
                        Status = "PENDING",
                        TotalAmount = (decimal)cart.CartItems.Sum(ci => (ci.Quantity ?? 0) * (ci.UnitPrice)),
                        CreatedAt = DateTime.Now
                    };

                    db.Orders.Add(newOrder);
                    db.SaveChanges();
                    // BƯỚC 2: CẬP NHẬT TỒN KHO & TẠO CHI TIẾT ĐƠN HÀNG
                    foreach (var item in cart.CartItems)
                    {
                        var variant = db.ProductVariants.Find(item.VariantId);

                        // Kiểm tra an toàn phút chót (vô cùng quan trọng)
                        if (variant == null || variant.Stock < item.Quantity)
                        {
                            throw new Exception($"Sản phẩm {variant?.Product?.ProductName} vừa mới hết hàng hoặc không đủ số lượng. Vui lòng kiểm tra lại giỏ hàng.");
                        }

                        // Cập nhật Stock
                        variant.Stock -= (item.Quantity ?? 0);

                        // Thêm vào chi tiết đơn hàng
                        db.OrderItems.Add(new OrderItem
                        {
                            OrderId = newOrder.OrderId,
                            VariantId = item.VariantId,
                            ProductName = variant.Product.ProductName,
                            SizeName = variant.Size.SizeCode,
                            ColorName = variant.Color?.ColorName,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        });
                    }
                    db.SaveChanges();

                    // BƯỚC 3: XÓA GIỎ HÀNG
                    db.CartItems.RemoveRange(cart.CartItems);
                    db.Carts.Remove(cart);
                    db.SaveChanges();

                    dbTransaction.Commit();

                    return RedirectToAction("OrderSuccess", new { code = newOrder.OrderCode });
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();

                    ModelState.AddModelError("", "Lỗi: " + ex.Message);

                    ViewBag.Cart = cart;
                    ViewBag.TotalAmount = cart.CartItems.Sum(ci => (ci.Quantity ?? 0) * (ci.UnitPrice));
                    return View("Checkout", user);
                }
            }
        }
        public ActionResult OrderSuccess(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy thông tin đơn hàng để hiển thị lời cảm ơn cá nhân hóa (tùy chọn)
            var order = db.Orders.FirstOrDefault(o => o.OrderCode == code);
            if (order == null) return RedirectToAction("Index", "Home");

            return View(order);
        }

        // GET: OrderItems
        public ActionResult Index()
        {
            // 1. Kiểm tra người dùng đã đăng nhập chưa
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "AppUsers");
            }

            // 2. Lấy thông tin user từ Session
            var user = (AppUser)Session["user"];
            int currentUserId = user.UserId;

            // 3. Lọc danh sách ĐƠN HÀNG của user này (Sắp xếp đơn mới nhất lên đầu)
            var myOrders = db.Orders
                             .Where(o => o.UserId == currentUserId)
                             .OrderByDescending(o => o.CreatedAt)
                             .ToList();

            return View(myOrders);
        }

        // GET: OrderItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            // Lấy tất cả sản phẩm thuộc đơn hàng này
            var orderItems = db.OrderItems
                               .Include(o => o.Order)
                               .Where(o => o.OrderId == id)
                               .ToList();

            if (orderItems == null || !orderItems.Any())
            {
                return HttpNotFound();
            }

            // Truyền OrderId vào ViewBag để hiển thị trên tiêu đề
            ViewBag.OrderCode = orderItems.FirstOrDefault().Order.OrderCode;

            return View(orderItems);
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
