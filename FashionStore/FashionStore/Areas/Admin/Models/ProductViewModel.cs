using System.ComponentModel.DataAnnotations;
using System.Web;

namespace FashionStore.Areas.Admin.Models
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã SKU")]
        [Display(Name = "Mã SKU")]
        public string SKU { get; set; }

        [Display(Name = "Đường dẫn (Slug)")]
        public string Slug { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá gốc")]
        [Display(Name = "Giá gốc")]
        public decimal BasePrice { get; set; }

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; }

        // Thay đổi từ int[] sang int (Single Select)
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        // Dùng để upload ảnh đại diện
        [Display(Name = "Ảnh đại diện")]
        public HttpPostedFileBase PrimaryImage { get; set; }
    }
}