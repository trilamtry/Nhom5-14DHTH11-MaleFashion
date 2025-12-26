using System.Collections.Generic;
using FashionStore.Models;

namespace FashionStore.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        // 1. Dữ liệu Thống kê nhanh
        public int TotalOrders { get; set; }
        public decimal TotalRevenueLast30Days { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }

        // 2. Danh sách Đơn hàng mới nhất (SỬ DỤNG MODEL ORDER CHÍNH THỨC)
        public List<Order> RecentOrders { get; set; }

        // 3. Cảnh báo Tồn kho thấp
        public List<ProductVariant> LowStockVariants { get; set; }
    }
}