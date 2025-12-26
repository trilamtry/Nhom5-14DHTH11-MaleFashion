using System.ComponentModel.DataAnnotations;

namespace FashionStore.Areas.Admin.Models
{
    public class ProductVariantViewModel
    {
        public int VariantId { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; } // Để hiển thị tên SP đang thêm
        public decimal BasePrice { get; set; }  // Để gợi ý giá

        [Display(Name = "Kích thước")]
        [Required(ErrorMessage = "Vui lòng chọn Size")]
        public int SizeId { get; set; }

        [Display(Name = "Màu sắc")]
        [Required(ErrorMessage = "Vui lòng chọn Màu")]
        public int ColorId { get; set; }

        [Display(Name = "Giá bán")]
        [Required]
        public decimal Price { get; set; }

        [Display(Name = "Số lượng tồn kho")]
        [Required]
        [Range(0, 10000, ErrorMessage = "Số lượng không hợp lệ")]
        public int Stock { get; set; }
    }
}