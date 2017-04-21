using System.Web.Mvc;
using System.Web.Routing;

namespace ResizingServer
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.LowercaseUrls = true;
            routes.MapRoute(
                name: "Upload",
                url: System.Configuration.ConfigurationManager.AppSettings["UploadRouteUrl"],
                defaults: new { controller = "Upload", action = "Index" }
            );
        }
    }
}
