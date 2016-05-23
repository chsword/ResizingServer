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
        [HttpGet]
        public ActionResult Index()
        {
            return Content("allow get");
        }
        [HttpPost]
        public ActionResult Index(string apiKey, string category, HttpPostedFileBase file)
        {
            if (_apiKey != apiKey) return Error("Invalid apiKey");
            var filename = file.FileName;
            if (
                !_allowAllExtenssions && 
                !_extensions.Any(c => filename.EndsWith(c)))
            {
                return Error("Invalid file extension");
            }
            if (_allowLocalIpUploadOnly)
            {
                if (!Current.IsLocalIp(Current.RequestIp))
                {
                    return Error("Invalid IP , only allow local ip to upload");
                }
            }
            if (string.IsNullOrWhiteSpace(category) || _allowFolders.All(c => category != c))
            {
                return Error("Invalid category");
            }
            var extenssion = Path.GetExtension(filename);
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

        #region Override Method

        private ActionResult Error(string message)
        {
            return Json(new {success = false, message});
        }

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

        #endregion

        #region Configure var

        private readonly string _apiKey = ConfigurationManager.AppSettings["ApiKey"];
        private readonly string[] _allowFolders = (ConfigurationManager.AppSettings["AllowFolders"]??"").Split(',');
        private readonly string[] _extensions= { ".jpg", ".png" };
        private readonly bool _allowAllExtenssions = (ConfigurationManager.AppSettings["AllowAllExtensions"] == "true");
        private readonly bool _allowLocalIpUploadOnly = (ConfigurationManager.AppSettings["AllowLocalIpUploadOnly"] == "true");

        #endregion
    }
}