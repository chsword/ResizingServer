using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public static string FormatUrl(string format, int width, int height, ResizingFormat outputFormat, ResizingMode mode = ResizingMode.Crop)
        {
            return FormatImageUrl(FormatUrl(format, width, height, mode), outputFormat);
        }

        public static string Format(string format, int width, int height, ResizingFormat outputFormat, ResizingMode mode = ResizingMode.Crop)
        {
            return FormatUrl(format, width, height, outputFormat, mode);
        }

        public static string FormatImageUrl(string url, ResizingFormat outputFormat)
        {
            var token = GetFormat(outputFormat);
            return token == null ? url : AppendQuery(url, "format=" + token);
        }

        public static string FormatTencentCdnUrl(string url, int width, int height, ResizingMode mode = ResizingMode.Crop)
        {
            return FormatTencentCdnUrl(url, width, height, ResizingFormat.Original, mode);
        }

        public static string FormatTencentCdnUrl(string url, int width, int height, ResizingFormat outputFormat, ResizingMode mode = ResizingMode.Crop)
        {
            ValidateResize(width, height, mode);
            var query = $"imageMogr2/thumbnail/{width}x{height}";
            switch (mode)
            {
                case ResizingMode.Crop:
                    query = $"imageMogr2/thumbnail/!{width}x{height}r/gravity/center/crop/{width}x{height}";
                    break;
                case ResizingMode.Pad:
                    query = $"{query}/pad/1";
                    break;
            }
            var token = GetFormat(outputFormat);
            if (token != null) query += "/format/" + token;
            return AppendQuery(url, query);
        }

        public static string FormatTencentCdnUrl(string url, ResizingFormat outputFormat)
        {
            var token = GetFormat(outputFormat);
            return token == null ? url : AppendQuery(url, "imageMogr2/format/" + token);
        }

        public static string FormatAliyunCdnUrl(string url, int width, int height, ResizingMode mode = ResizingMode.Crop)
        {
            return FormatAliyunCdnUrl(url, width, height, ResizingFormat.Original, mode);
        }

        public static string FormatAliyunCdnUrl(string url, int width, int height, ResizingFormat outputFormat, ResizingMode mode = ResizingMode.Crop)
        {
            ValidateResize(width, height, mode);
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

            var query = $"x-oss-process=image/resize,{modeToken},w_{width},h_{height}";
            var token = GetFormat(outputFormat);
            if (token != null) query += "/format," + token;
            return AppendQuery(url, query);
        }

        public static string FormatAliyunCdnUrl(string url, ResizingFormat outputFormat)
        {
            var token = GetFormat(outputFormat);
            return token == null ? url : AppendQuery(url, "x-oss-process=image/format," + token);
        }

        [Obsolete("This overload uses a legacy custom x-amz-process protocol, not an AWS API. Use the endpoint, bucket, key overload for AWS Dynamic Image Transformation.")]
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

        public static string FormatAwsCdnUrl(string endpoint, string bucket, string key, int width, int height,
            ResizingFormat outputFormat = ResizingFormat.Original, ResizingMode mode = ResizingMode.Crop)
        {
            ValidateResize(width, height, mode);
            var fit = mode == ResizingMode.Crop ? "cover" : mode == ResizingMode.Max ? "inside" : "contain";
            var edits = new JObject
            {
                ["resize"] = new JObject { ["width"] = width, ["height"] = height, ["fit"] = fit }
            };
            return BuildAwsUrl(endpoint, bucket, key, outputFormat, edits);
        }

        public static string FormatAwsCdnUrl(string endpoint, string bucket, string key, ResizingFormat outputFormat)
        {
            return BuildAwsUrl(endpoint, bucket, key, outputFormat, new JObject());
        }

        static string BuildAwsUrl(string endpoint, string bucket, string key, ResizingFormat outputFormat, JObject edits)
        {
            Uri uri;
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) ||
                !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) ||
                !string.IsNullOrEmpty(uri.UserInfo))
                throw new ArgumentException("Use an HTTP(S) handler endpoint without a query, fragment or credentials.", nameof(endpoint));
            if (string.IsNullOrWhiteSpace(bucket)) throw new ArgumentException("A source bucket is required.", nameof(bucket));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("An object key is required.", nameof(key));
            var token = GetFormat(outputFormat);
            if (token != null) edits["toFormat"] = outputFormat == ResizingFormat.Jpeg ? "jpeg" : token;
            var request = new JObject { ["bucket"] = bucket, ["key"] = key, ["edits"] = edits };
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(request.ToString(Formatting.None)));
            return endpoint.TrimEnd('/') + "/" + encoded;
        }

        static string GetFormat(ResizingFormat format)
        {
            switch (format)
            {
                case ResizingFormat.Original: return null;
                case ResizingFormat.Jpeg: return "jpg";
                case ResizingFormat.Png: return "png";
                case ResizingFormat.WebP: return "webp";
                default: throw new ArgumentOutOfRangeException(nameof(format));
            }
        }

        static void ValidateResize(int width, int height, ResizingMode mode)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (mode != ResizingMode.Crop && mode != ResizingMode.Max && mode != ResizingMode.Pad)
                throw new ArgumentOutOfRangeException(nameof(mode));
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
            var separator = urlWithoutFragment.IndexOf('?') < 0 ? "?" :
                urlWithoutFragment.EndsWith("?", StringComparison.Ordinal) ||
                urlWithoutFragment.EndsWith("&", StringComparison.Ordinal) ? string.Empty : "&";
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
