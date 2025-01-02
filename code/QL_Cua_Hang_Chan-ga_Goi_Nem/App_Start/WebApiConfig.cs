using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace QL_Cua_Hang_Chan_ga_Goi_Nem
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
