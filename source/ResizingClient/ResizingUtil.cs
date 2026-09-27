using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ResizingClient
{
    public class ResizingUtil
    {
        public static string Host { get; set; } = System.Configuration.ConfigurationManager.AppSettings["ResizingServer.Host"];
        public static string ApiKey { get; set; } = System.Configuration.ConfigurationManager.AppSettings["ResizingServer.ApiKey"];

        public static string UploadUrl { get; set; } = System.Configuration.ConfigurationManager.AppSettings["ResizingServer.UploadUrl"];

        public static string FormatUrl(string format, int width, int height, ResizingMode mode = ResizingMode.Crop)
        {
            return $"{Host}{string.Format(format, width, height, GetMode(mode))}";
        }

        public static string Format(string format, int width, int height, ResizingMode mode = ResizingMode.Crop)
        {
            return FormatUrl(format, width, height, mode);
        }

        public static string FormatTencentCdnUrl(string url, int width, int height, ResizingMode mode = ResizingMode.Crop)
        {
            var query = $"imageMogr2/thumbnail/{width}x{height}";
            switch (mode)
            {
                case ResizingMode.Crop:
                    query = $"{query}/gravity/center/crop/{width}x{height}";
                    break;
                case ResizingMode.Pad:
                    query = $"{query}/pad/1";
                    break;
            }
            return AppendQuery(url, query);
        }

        public static string FormatAliyunCdnUrl(string url, int width, int height, ResizingMode mode = ResizingMode.Crop)
        {
            var modeToken = "m_fill";
            switch (mode)
            {
                case ResizingMode.Max:
                    modeToken = "m_lfit";
                    break;
                case ResizingMode.Pad:
                    modeToken = "m_pad";
                    break;
            }

            return AppendQuery(url, $"x-oss-process=image/resize,{modeToken},w_{width},h_{height}");
        }

        public static string FormatAwsCdnUrl(string url, int width, int height, ResizingMode mode = ResizingMode.Crop)
        {
            var fitToken = "m_cover";
            switch (mode)
            {
                case ResizingMode.Max:
                    fitToken = "m_fit";
                    break;
                case ResizingMode.Pad:
                    fitToken = "m_pad";
                    break;
            }

            return AppendQuery(url, $"x-amz-process=image/resize,w_{width},h_{height},{fitToken}");
        }

        public static Task<UploadResult> Upload(Stream stream, string filename, string category)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            return Upload(bytes, filename, category);
        }

        public static async Task<UploadResult> Upload(byte[] bytes, string filename, string category)
        {
            using (var client = new HttpClient())
            {
                using (var content =
                       new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                {
                    content.Add(new StreamContent(new MemoryStream(bytes)), "file", filename);

                    using (
                        var message = await client.PostAsync($"{UploadUrl}?apikey={ApiKey}&category={category}", content))
                    {
                        var input = await message.Content.ReadAsStringAsync();

                        return JsonConvert.DeserializeObject<UploadResult>(input);
                    }
                }
            }
        }

        static string AppendQuery(string url, string query)
        {
            if (string.IsNullOrWhiteSpace(url)) return url;
            var fragmentIndex = url.IndexOf('#');
            var fragment = fragmentIndex >= 0 ? url.Substring(fragmentIndex) : string.Empty;
            var urlWithoutFragment = fragmentIndex >= 0 ? url.Substring(0, fragmentIndex) : url;
            var separator = urlWithoutFragment.IndexOf('?') >= 0 ? "&" : "?";
            return $"{urlWithoutFragment}{separator}{query}{fragment}";
        }

        static string GetMode(ResizingMode mode)
        {
            switch (mode)
            {
                case ResizingMode.Crop:
                    return "c";
                case ResizingMode.Max:
                    return "m";
                case ResizingMode.Pad:
                    return "p";
            }
            return "c";
        }
    }
}
