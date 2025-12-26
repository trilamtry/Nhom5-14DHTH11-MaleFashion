using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using FashionStore.Models;

namespace FashionStore.Areas.Admin.Controllers
{
    public class AppUserController : Controller
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        // ----------------------------------------
        // GET: Admin/AppUser/Index (Danh sách Khách hàng)
        // ----------------------------------------
        public ActionResult Index(string search)
        {
            var users = db.AppUsers.OrderByDescending(u => u.CreatedAt).AsQueryable();

            // Tìm kiếm theo Email hoặc SĐT
            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.Email.Contains(search) || u.Phone.Contains(search) || u.FullName.Contains(search));
            }

            return View(users.ToList());
        }

        // ----------------------------------------
        // GET: Admin/AppUser/Details/5 (Chi tiết + Lịch sử đơn hàng)
        // ----------------------------------------
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Load User kèm theo danh sách Đơn hàng (Orders)
            var user = db.AppUsers
                         .Include(u => u.Orders)
                         .SingleOrDefault(u => u.UserId == id); // Lưu ý: Property trong Model là AppUserId

            if (user == null) return HttpNotFound();

            return View(user);
        }

        // ----------------------------------------
        // POST: Admin/AppUser/ToggleStatus (Khóa/Mở khóa)
        // ----------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var user = db.AppUsers.Find(id);
            if (user != null)
            {
                // Đảo ngược trạng thái (Đang mở -> Khóa, Đang khóa -> Mở)
                user.IsActive = !user.IsActive;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}