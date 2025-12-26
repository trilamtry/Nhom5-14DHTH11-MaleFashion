using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using FashionStore.Models;

namespace FashionStore.Areas.Admin.Controllers
{
    public class ColorController : Controller
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        // GET: Admin/Color
        public ActionResult Index()
        {
            return View(db.Colors.ToList());
        }

        // GET: Admin/Color/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Color/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ColorId,ColorHex,ColorName")] Color color)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng Mã Hex
                if (db.Colors.Any(c => c.ColorHex == color.ColorHex))
                {
                    ModelState.AddModelError("ColorHex", "Mã màu Hex này đã tồn tại.");
                    return View(color);
                }

                db.Colors.Add(color);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(color);
        }

        // GET: Admin/Color/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Color color = db.Colors.Find(id);
            if (color == null) return HttpNotFound();
            return View(color);
        }

        // POST: Admin/Color/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ColorId,ColorHex,ColorName")] Color color)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng Mã Hex (trừ chính nó)
                if (db.Colors.Any(c => c.ColorHex == color.ColorHex && c.ColorId != color.ColorId))
                {
                    ModelState.AddModelError("ColorHex", "Mã màu Hex này đã tồn tại.");
                    return View(color);
                }

                db.Entry(color).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(color);
        }

        // GET: Admin/Color/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Color color = db.Colors.Find(id);
            if (color == null) return HttpNotFound();
            return View(color);
        }

        // POST: Admin/Color/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Color color = db.Colors.Find(id);
            // Kiểm tra xem Màu này có đang được sử dụng trong ProductVariant không
            bool isUsed = db.ProductVariants.Any(v => v.ColorId == id);

            if (isUsed)
            {
                ModelState.AddModelError("", "Màu này đang được sử dụng trong sản phẩm, không thể xóa!");
                return View(color);
            }

            db.Colors.Remove(color);
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