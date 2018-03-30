using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ResizingServer
{
    public class HttpModule1 : IHttpModule
    {

        /// <summary>  
        /// 处置由实现 System.Web.IHttpModule 的模块使用的资源（内存除外）  
        /// </summary>  
        public void Dispose() { }

        /// <summary>  
        /// 初始化模块，并使其为处理请求做好准备。  
        /// </summary>  
        /// <param name="context"></param>  
        public void Init(HttpApplication context)
        {
            context.BeginRequest += context_BeginRequest;
            context.EndRequest += context_EndRequest;    
          
        }

        //private void BeginRequest_EndEvent(IAsyncResult ar)
        //{ 
        //}

        //private IAsyncResult BeginRequest_BeginEvent(object sender, EventArgs e, AsyncCallback cb, object extraData)
        //{
        //    HttpApplication application = sender as HttpApplication;
        //    HttpContext context = application.Context;
        //    HttpRequest request = application.Request;
        //    HttpResponse response = application.Response;

        //}

        /// <summary>  
        /// 在 ASP.NET 响应请求时作为 HTTP 执行管线链中的最后一个事件发生。  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        void context_EndRequest(object sender, EventArgs e)
        {
            HttpApplication application = sender as HttpApplication;
            HttpContext context = application.Context;
            HttpRequest request = application.Request;
            HttpResponse response = application.Response;

            response.Write("context_EndRequest >> 在 ASP.NET 响应请求时作为 HTTP 执行管线链中的最后一个事件发生");
        }

        /// <summary>  
        /// 在 ASP.NET 响应请求时作为 HTTP 执行管线链中的第一个事件发生。  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication application = sender as HttpApplication;
            HttpContext context = application.Context;
            HttpRequest request = application.Request;
            HttpResponse response = application.Response;
            var absoluteUrl = VirtualPathUtility.ToAbsolute("~/u/");
            // 0246 guid
            // 0 1 日 2 3年 4 5月 6 7Width 8Height 9Mode 10 ext  +2
            var regex = new Regex(@"/u/(\w+)/(\w{3})(\d{2})(\w{7})(\d{2})(\w{7})(\d{2})(\w{15})(\d+)x(\d+)(\w{1})\.(jpg|png)", RegexOptions.Compiled);
            var path = request.Url.LocalPath;

                if (path.StartsWith("/u/", StringComparison.OrdinalIgnoreCase) &&
                    path.Last() == 'g')
                {
                var match = regex.Match(path);
          var width = GetSizeValue(match.Groups[9].Value);
              var height = GetSizeValue(match.Groups[10].Value);
                //ev.QueryString["scale"] = "both";
      var mode = GetMode(match.Groups[11].Value);
                var filename = $"/upload/{match.Groups[1].Value}/{match.Groups[5].Value}{match.Groups[7].Value}/{match.Groups[3].Value}/{match.Groups[2].Value}{match.Groups[4].Value}{match.Groups[6].Value}{match.Groups[8].Value}.{match.Groups[12].Value}";
                    context.RewritePath(filename);
                }
       
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