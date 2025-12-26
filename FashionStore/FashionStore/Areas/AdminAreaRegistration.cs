using System.Web.Mvc;

namespace FashionStore.Areas.Admin
{
    // Đảm bảo namespace là Fashion__Store.Areas.Admin
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin"; // Tên Area là Admin
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            // Định nghĩa tuyến đường (route) cho Area Admin
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { controller = "Admin", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "FashionStore.Areas.Admin.Controllers" } // Quan trọng: Chỉ định namespace của các controller trong Area
            );
        }
    }
}