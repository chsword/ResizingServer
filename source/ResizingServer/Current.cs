using System.Text.RegularExpressions;
using System.Web;
using System.Web.WebPages;

namespace ResizingServer
{
    public static class Current
    {
        /// <summary>
        /// Shortcut to HttpContext.Current.
        /// </summary>
        public static HttpContextBase Context => new HttpContextWrapper(HttpContext.Current);

        /// <summary>
        /// Shortcut to HttpContext.Current.Request.
        /// </summary>
        public static HttpRequestBase Request => Context.Request;

        /// <summary>
        /// Whether to render chart images at double resolution or not
        /// </summary>
        /// <remarks>Long-term, this will need updating to observe the pixel ratio which will grow to higher than 2 as displays improve</remarks>
        public static bool IsHighDpi
        {
            get
            {
                // HACK, Hackity hack hack hack, but it works for now.
                var cookie = Request.Cookies["highDPI"];
                return cookie != null && cookie.Value == "true";
            }
        }

        /// <summary>
        /// Gets if the current request is for a mobile view
        /// </summary>
        public static bool IsMobile => Context.GetOverriddenBrowser().IsMobileDevice;

        private static readonly Regex LastIpAddress = new Regex(@"\b([0-9]{1,3}\.){3}[0-9]{1,3}$",
                                                                RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Gets the IP this request came from, gets the real IP when behind a proxy
        /// </summary>
        public static string RequestIp
        {
            get
            {
                var ip = Request.ServerVariables["REMOTE_ADDR"];
                var ipForwarded = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                var realIp = Request.ServerVariables["HTTP_X_REAL_IP"];
                if (!string.IsNullOrWhiteSpace(realIp)
                    && Request.ServerVariables["HTTP_X_FORWARDED_PROTO"] == "https")
                    return realIp;

                // check if we were forwarded from a proxy
                if (!string.IsNullOrWhiteSpace(ipForwarded))
                {
                    ipForwarded = LastIpAddress.Match(ipForwarded).Value;
                    if (!string.IsNullOrWhiteSpace(ipForwarded)
                        && !IsLocalIp(ipForwarded))
                        ip = ipForwarded;
                }
                return !string.IsNullOrWhiteSpace(ip) ? ip : "0.0.0.0";
            }
        }

        public static bool IsLocalIp(string s)
        {
            return (s.StartsWith("192.168.") || s.StartsWith("10.") || s.StartsWith("127.0.0."));
        }
    }
}