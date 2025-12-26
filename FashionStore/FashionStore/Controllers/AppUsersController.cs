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
    public class AppUsersController : Controller
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        public ActionResult Logout()
        {
            Session["user"] = null;
            return RedirectToAction("Login");
        }
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(AppUser model)
        {
            var user = db.AppUsers.FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == model.PasswordHash);

            if (user != null)
            {
                if (!user.IsActive)
                {
                    ViewBag.ThongBao = "Tài khoản của bạn đã bị khóa.";
                    return View();
                }
                Session["user"] = user;

                var userRole = user.Roles.FirstOrDefault();
                if (userRole != null && userRole.RName == "ADMIN")
                {
                    return RedirectToAction("Index", "Admin", new { area = "Admin" });
                }

                return RedirectToAction("Index", "Products");
            }
            ViewBag.ThongBao = "Email hoặc mật khẩu không chính xác.";
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(AppUser model, string ConfirmPassword)
        {
            bool hasError = false;

            // Email trùng
            if (db.AppUsers.Any(x => x.Email == model.Email))
            {
                ViewBag.ThongBaoEmail = "Email đã được sử dụng";
                hasError = true;
            }

            // SĐT trùng
            if (db.AppUsers.Any(x => x.Phone == model.Phone))
            {
                ViewBag.ThongBaoPhone = "Số điện thoại đã được sử dụng";
                hasError = true;
            }

            // Confirm password khớp
            if (model.PasswordHash != ConfirmPassword)
            {
                ViewBag.ThongBaoConfirm = "Mật khẩu nhập lại không khớp";
                hasError = true;
            }

            // Mật khẩu mạnh (ít nhất 8 ký tự, chữ hoa, chữ thường, số, ký tự đặc biệt)
            var regex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$");
            if (!regex.IsMatch(model.PasswordHash))
            {
                ViewBag.ThongBaoPassword = "Mật khẩu phải có chữ hoa, chữ thường, số và ký tự đặc biệt, tối thiểu 8 ký tự";
                hasError = true;
            }

            // Nếu có lỗi, trả về view
            if (hasError)
            {
                return View(model);
            }
            if (!ModelState.IsValid || hasError)
            {
                model.IsActive = true;
                model.CreatedAt = DateTime.Now;
                model.PasswordHash = model.PasswordHash;
            }
            // Ghi DB                      

            db.AppUsers.Add(model);
            db.SaveChanges();

            return RedirectToAction("Login");
        }

        // GET: AppUsers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["user"] == null) {
                return RedirectToAction("Login");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppUser appUser = db.AppUsers.Find(id);
            if (appUser == null)
            {
                return HttpNotFound();
            }
            return View(appUser);
        }

        // POST: AppUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,Email,PasswordHash,FullName,Phone,IsActive,CreatedAt")] AppUser appUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(appUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(appUser);
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
