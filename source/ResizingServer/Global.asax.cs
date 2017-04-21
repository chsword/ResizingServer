using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using ImageResizer.Configuration;

namespace ResizingServer
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            var absoluteUrl = VirtualPathUtility.ToAbsolute("~/u/");
            // 0246 guid
            // 0 1 日 2 3年 4 5月 6 7Width 8Height 9Mode 10 ext  +2
            var regex = new Regex(@"/u/(\w+)/(\w{3})(\d{2})(\w{7})(\d{2})(\w{7})(\d{2})(\w{15})(\d+)x(\d+)(\w{1})\.(jpg|png)", RegexOptions.Compiled);
            Config.Current.Pipeline.Rewrite += delegate (IHttpModule sender, HttpContext context, IUrlEventArgs ev)
            {
                if (ev.VirtualPath.StartsWith(absoluteUrl, StringComparison.OrdinalIgnoreCase) &&
                    ev.VirtualPath.Last() == 'g')
                {
                    // )@"/images/([0-9]+)/([0-9]+)/([^/]+)\.(jpg|png)"
                    ev.VirtualPath = regex.Replace(ev.VirtualPath, delegate (Match match)
                    {
                        ev.QueryString["width"] = GetSizeValue(match.Groups[9].Value);
                        ev.QueryString["height"] = GetSizeValue(match.Groups[10].Value);
                        //ev.QueryString["scale"] = "both";
                        ev.QueryString["mode"] = GetMode(match.Groups[11].Value);
                        return
                            $"/upload/{match.Groups[1].Value}/{match.Groups[5].Value}{match.Groups[7].Value}/{match.Groups[3].Value}/{match.Groups[2].Value}{match.Groups[4].Value}{match.Groups[6].Value}{match.Groups[8].Value}.{match.Groups[12].Value}";

                    });
                    context.RewritePath(ev.VirtualPath);
                }
            };
        }

        private string GetSizeValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "0") return "10000";
            return value;
        }

        private string GetMode(string value)
        {
            switch (value)
            {
                case "c":
                    return "crop";
                case "m":
                    return "max";
                case "p":
                    return "pad";
            }
            return "crop";
        }
    }
}