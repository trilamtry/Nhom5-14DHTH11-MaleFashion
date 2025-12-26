using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using FashionStore.Models;

namespace FashionStore.Areas.Admin.Controllers
{
    public class SizeController : Controller
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        // GET: Admin/Size
        public ActionResult Index()
        {
            // Sắp xếp theo SortOrder để hiển thị đúng trình tự (S, M, L...)
            return View(db.Sizes.OrderBy(s => s.SortOrder).ToList());
        }

        // GET: Admin/Size/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Size/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // ĐÃ SỬA: Loại bỏ IsActive khỏi Bind
        public ActionResult Create([Bind(Include = "SizeId,SizeCode,SortOrder")] Size size)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng tên Size
                if (db.Sizes.Any(s => s.SizeCode == size.SizeCode))
                {
                    ModelState.AddModelError("SizeCode", "Tên Size này đã tồn tại.");
                    return View(size);
                }

                db.Sizes.Add(size);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(size);
        }

        // GET: Admin/Size/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Size size = db.Sizes.Find(id);
            if (size == null) return HttpNotFound();
            return View(size);
        }

        // POST: Admin/Size/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // ĐÃ SỬA: Loại bỏ IsActive khỏi Bind
        public ActionResult Edit([Bind(Include = "SizeId,SizeCode,SortOrder")] Size size)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng tên (trừ chính nó)
                if (db.Sizes.Any(s => s.SizeCode == size.SizeCode && s.SizeId != size.SizeId))
                {
                    ModelState.AddModelError("SizeCode", "Tên Size này đã tồn tại.");
                    return View(size);
                }

                db.Entry(size).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(size);
        }

        // GET: Admin/Size/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Size size = db.Sizes.Find(id);
            if (size == null) return HttpNotFound();
            return View(size);
        }

        // POST: Admin/Size/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Size size = db.Sizes.Find(id);
            // Kiểm tra xem Size này có đang được sử dụng trong ProductVariant không
            bool isUsed = db.ProductVariants.Any(v => v.SizeId == id);

            if (isUsed)
            {
                ModelState.AddModelError("", "Size này đang được sử dụng trong sản phẩm, không thể xóa!");
                return View(size);
            }

            db.Sizes.Remove(size);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}