using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Linq;
using System.Net.Http.Headers; 

namespace FashionStore.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // 3. Quan trọng: Ép dữ liệu trả về luôn là dạng JSON để các ứng dụng khác đọc được
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes
                                .FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            // 4. Khử lỗi "Vòng lặp vô tận" (Do quan hệ bảng trong file .edmx của bạn)
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling
                = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling
                = Newtonsoft.Json.PreserveReferencesHandling.None;
        }
    }
}