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
    public class BlogsController : Controller
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        // GET: Blogs
        public ActionResult Index()
        {
            return View(db.Blogs.ToList());
        }
        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult SendContact(string FullName, string Email, string MessageText)
        {
            try
            {
                var contact = new ContactMessage();
                contact.FullName = FullName;
                contact.Email = Email;
                contact.MessageText = MessageText;
                contact.CreatedAt = DateTime.Now;
                contact.IsRead = false;

                db.ContactMessages.Add(contact);
                db.SaveChanges();

                TempData["Message"] = "Gửi thành công!";
                return RedirectToAction("Contact");
            }
            catch (Exception ex)
            {
                return Content("Lỗi: " + ex.Message);
            }
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
