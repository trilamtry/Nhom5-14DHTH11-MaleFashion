using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Web.Mvc;
using FashionStore.Models;
using System.Data.Entity;

namespace FashionStore.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        // GET: Admin/Category/Index
        public ActionResult Index()
        {
            var categories = db.Categories.Include(c => c.CategoryGroup)
                                          .OrderBy(c => c.CategoryGroup.SortOrder)
                                          .ThenBy(c => c.SortOrder)
                                          .ToList();
            return View(categories);
        }

        // ================== CREATE ==================
        public ActionResult Create()
        {
            // Lấy danh sách Nhóm (Áo, Quần...) để hiển thị Dropdown
            ViewBag.GroupId = new SelectList(db.CategoryGroups, "GroupId", "GroupName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "GroupId,CategorySlug,CatName,SortOrder,IsActive")] Category category)
        {
            if (ModelState.IsValid)
            {
                // Tự tạo Slug nếu bỏ trống
                if (string.IsNullOrEmpty(category.CatSlug))
                {
                    category.CatSlug = GenerateSlug(category.CatName);
                }

                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.GroupId = new SelectList(db.CategoryGroups, "GroupId", "GroupName", category.GroupId);
            return View(category);
        }

        // ================== EDIT ==================
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Category category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            // Load lại Dropdown và chọn giá trị hiện tại
            ViewBag.GroupId = new SelectList(db.CategoryGroups, "GroupId", "GroupName", category.GroupId);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryId,GroupId,CatSlug,CatName,SortOrder,IsActive")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(category.CatSlug))
                {
                    category.CatSlug = GenerateSlug(category.CatName);
                }

                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.GroupId = new SelectList(db.CategoryGroups, "GroupId", "GroupName", category.GroupId);
            return View(category);
        }

        // ================== DELETE ==================
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Category category = db.Categories.Include(c => c.CategoryGroup).SingleOrDefault(c => c.CategoryId == id);
            if (category == null) return HttpNotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            // Lưu ý: Cần xử lý nếu danh mục đang có sản phẩm (Set null hoặc xóa)
            // Tạm thời xóa trực tiếp:
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Hàm tạo Slug (Copy từ ProductController sang hoặc để vào class Utility dùng chung)
        public string GenerateSlug(string phrase)
        {
            string str = RemoveAccent(phrase).ToLower();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }

        private string RemoveAccent(string txt)
        {
            byte[] bytes = Encoding.GetEncoding(1251).GetBytes(txt);
            return Encoding.ASCII.GetString(bytes);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}