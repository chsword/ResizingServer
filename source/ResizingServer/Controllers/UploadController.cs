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
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {

            return new JsonNetResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }

        private readonly string _apiKey = ConfigurationManager.AppSettings["ApiKey"];
        private readonly string[] _allowFolders = (ConfigurationManager.AppSettings["AllowFolders"]??"").Split(',');
        private readonly string[] _extensions= new[] { ".jpg", ".png" };
        private readonly bool _allowAllExtenssions = (ConfigurationManager.AppSettings["AllowAllExtensions"] == "true");
        
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
            if (
                !_allowAllExtenssions && 
                !_extensions.Any(c => filename.EndsWith(c)))
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
                RawUrl = path.RawPath

            });
        }
    }
}