using FashionStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FashionStore.Areas.Admin.Models;
using System.Data.Entity;

namespace FashionStore.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        public ActionResult Index()
        {
            DateTime last30Days = DateTime.Now.AddDays(-30);

            // 1. Truy vấn Dữ liệu Thống kê
            int totalOrders = db.Orders.Count();

            // Sử dụng Order và TotalAmount
            decimal totalRevenueLast30Days = db.Orders
                .Where(o => o.CreatedAt >= last30Days && o.Status == "COMPLETED")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            int totalProducts = db.Products.Count();
            int totalCustomers = db.AppUsers.Count(); // Sử dụng AppUser

            // 2. Lấy 5 đơn hàng mới nhất (Sử dụng Model Order)
            var recentOrders = db.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToList();

            // 3. Lấy 3-5 biến thể sắp hết hàng (Stock < 10)
            var lowStockVariants = db.ProductVariants
                .Where(v => v.Stock < 10)
                .OrderBy(v => v.Stock)
                .Take(5)
                .Include(v => v.Product)
                .Include(v => v.Size)
                .Include(v => v.Color)
                .ToList();

            // Tạo ViewModel sử dụng Model chính thức
            var viewModel = new AdminDashboardViewModel
            {
                TotalOrders = totalOrders,
                TotalRevenueLast30Days = totalRevenueLast30Days,
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,

                // SỬ DỤNG MODEL ORDER THAY VÌ DONHANG
                RecentOrders = recentOrders,

                LowStockVariants = lowStockVariants
            };

            // Trả về View Index.cshtml của Area Admin
            return View(viewModel);
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