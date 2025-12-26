using System;

namespace FashionStore.Models
{
    public class ProductReviewViewModel
    {
        public int ReviewId { get; set; }

        public string ReviewCode { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        // Thông tin sản phẩm
        public string ProductName { get; set; }

        public string SizeName { get; set; }

        public string ColorName { get; set; }

        // Thông tin đơn hàng
        public string OrderCode { get; set; }

        // Thông tin khách hàng
        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }

        public string CustomerPhone { get; set; }
    }
}