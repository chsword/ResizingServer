using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ResizingClient;

namespace ResizingServer.Controllers
{
    public class UploadController : Controller
    {
        private readonly string _apiKey = ConfigurationManager.AppSettings["ApiKey"];
        private readonly string[] _allowFolders = (ConfigurationManager.AppSettings["AllowFolders"]??"").Split(',');
        private readonly string[] _extensions= new[] { ".jpg", ".png" };

        private ActionResult Error(string message)
        {
            return Json(new {success = false, message});
        }

        [HttpGet]
        public ActionResult Index()
        {
            return Content("allow get");
        }
        [HttpPost]
        public ActionResult Index(string apiKey, string category, HttpPostedFileBase file)
        {
            if (_apiKey != apiKey) return Error("error api key");
            var filename = file.FileName;
            if (!_extensions.Any(c => filename.EndsWith(c)))
            {
                return Error("error extensions");
            }
            if (string.IsNullOrWhiteSpace(category) || _allowFolders.All(c => category != c))
            {
                return Error("error category");
            }
            var extenssion = Path.GetExtension(filename);
            if (string.IsNullOrWhiteSpace(extenssion) || _extensions.All(c => extenssion != c))
            {
                return Error("error extensions");
            }
            var path = new ResizingPath(category,extenssion);
        
            var physicalPath = Server.MapPath(path.PhysicalPath);
            if (!Directory.Exists(physicalPath))
            {
                Directory.CreateDirectory(physicalPath);
            }
            var physicalFilename= Server.MapPath(path.PhysicalFilename);
            file.SaveAs(physicalFilename);

            return Json(new UploadResult
            {
                IsSuccess = true,
                FormatUrl = path.VirtualFormatFilename,
                RawUrl = ResizingUtil.FormatUrl(path.VirtualFormatFilename, 0, 0)

            });
        }
    }

    public class ResizingPath
    {
        public string Year { get;private set; }
        public string Month { get; private set; }
        public string Date { get; private set; }
        public string Category { get; private set; }
        public string PhysicalPath { get; private set; }
        //public string VirtualPath { get; private set; }
        public string PhysicalFilename { get; private set; }
        //public string VirtualFilename { get; private set; }
        public string VirtualFormatFilename { get; private set; }
        public ResizingPath(string category,string extension)
        {
            // 0246 guid
            // 0 1 日 2 3年 4 5月 6 7Width 8Height 9Mode 10 ext  +2
            var guid = Guid.NewGuid().ToString("n");
            string guid1 = guid.Substring(0, 3),
                guid2 = guid.Substring(3, 7),
                guid3 = guid.Substring(10, 7),
                guid4 = guid.Substring(17, 15);
            Year = DateTime.Now.ToString("yy");
            Month = DateTime.Now.ToString("MM");
            Date = DateTime.Now.ToString("dd");
            Category = category;
            PhysicalPath = $"~/upload/{category}/{Year}{Month}/{Date}";
            PhysicalFilename = $"{PhysicalPath}/{guid}{extension}";
            VirtualFormatFilename =
                $"/u/{category}/{guid1}{Date}{guid2}{Year}{guid3}{Month}{guid4}{{0}}x{{1}}{{2}}{extension}";
        }
    }
}