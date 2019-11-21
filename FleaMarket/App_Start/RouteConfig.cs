using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FleaMarket
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                namespaces: new[] {"FleaMarket.Controllers"},
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                //namespace使用的数据，不过需要使用 new 来创建
                namespaces: new[] {"FleaMarket.Controllers.API"},
                name: "API",
                url: "api/{controller}/{action}/{id}"
            );
        }
    }
}