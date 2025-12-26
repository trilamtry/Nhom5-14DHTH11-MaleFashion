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
    public class CartItemsController : Controller   
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();
        [HttpPost]
        public ActionResult AddToCart(int variantId, int quantity)
        {
            // 1. Tìm biến thể sản phẩm và kiểm tra tồn kho cơ bản
            var variant = db.ProductVariants.Include(v => v.Product).FirstOrDefault(v => v.VariantId == variantId);
            if (variant == null) return HttpNotFound();

            if (quantity <= 0) return RedirectToAction("Index", "Products");

            Cart cart;

            // 2. Xác định giỏ hàng (Đã đăng nhập hoặc Giỏ hàng tạm)
            if (Session["user"] != null)
            {
                var user = (AppUser)Session["user"];
                cart = db.Carts.FirstOrDefault(c => c.UserId == user.UserId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        CartToken = Guid.NewGuid().ToString(),
                        UserId = user.UserId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    db.Carts.Add(cart);
                    db.SaveChanges();
                }
            }
            else
            {
                string cartToken = Request.Cookies["CartToken"]?.Value;
                if (string.IsNullOrEmpty(cartToken))
                {
                    cartToken = Guid.NewGuid().ToString();
                    Response.Cookies.Add(new HttpCookie("CartToken", cartToken) { Expires = DateTime.Now.AddDays(7) });
                }

                cart = db.Carts.FirstOrDefault(c => c.CartToken == cartToken);
                if (cart == null)
                {
                    cart = new Cart
                    {
                        CartToken = cartToken,
                        UserId = null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    db.Carts.Add(cart);
                    db.SaveChanges();
                }
            }

            // 3. KIỂM TRA SỐ LƯỢNG TỒN KHO TRƯỚC KHI THÊM
            var cartItem = db.CartItems.FirstOrDefault(ci => ci.CartId == cart.CartId && ci.VariantId == variantId);

            // Tính tổng số lượng nếu thêm mới
            int currentInCart = cartItem?.Quantity ?? 0;
            int totalRequested = currentInCart + quantity;

            if (totalRequested > variant.Stock)
            {
                // Thông báo lỗi (Sử dụng TempData để hiển thị ở trang Index)
                TempData["Error"] = $"Sản phẩm {variant.Product.ProductName} chỉ còn {variant.Stock} sản phẩm. Bạn hiện có {currentInCart} trong giỏ.";
                return RedirectToAction("Index", "Products");
            }

            // 4. Thực hiện thêm hoặc cập nhật
            if (cartItem != null)
            {
                cartItem.Quantity = totalRequested;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    VariantId = variantId,
                    Quantity = quantity,
                    UnitPrice = variant.Price
                };
                db.CartItems.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        public ActionResult RemoveFromCart(int cartItemId)
        {
            CartItem cartItem = null;

            if (Session["user"] != null) // đã đăng nhập
            {
                var user = (AppUser)Session["user"];
                cartItem = db.CartItems
                    .FirstOrDefault(ci => ci.CartItemId == cartItemId && ci.Cart.UserId == user.UserId);
            }
            else // chưa đăng nhập → giỏ hàng tạm
            {
                string cartToken = Request.Cookies["CartToken"]?.Value;
                if (!string.IsNullOrEmpty(cartToken))
                {
                    cartItem = db.CartItems
                        .FirstOrDefault(ci => ci.CartItemId == cartItemId && ci.Cart.CartToken == cartToken);
                }
            }

            if (cartItem != null)
            {
                db.CartItems.Remove(cartItem);
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Products");
        }
        // GET: CartItems
        public ActionResult Index()
        {
            // 1. Kiểm tra người dùng đã đăng nhập chưa
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "AppUsers");
            }

            // 2. Lấy thông tin user từ Session
            var user = (AppUser)Session["user"];

            // 3. Lọc CartItems: Chỉ lấy những item thuộc giỏ hàng của UserId này
            var cartItems = db.CartItems
                              .Include(c => c.Cart)
                              .Include(c => c.ProductVariant)
                              .Include(c => c.ProductVariant.Product) // Load thêm thông tin sản phẩm nếu cần
                              .Where(c => c.Cart.UserId == user.UserId)
                              .ToList();

            return View(cartItems);
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
