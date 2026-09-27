using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ResizingClient.Tests
{
    [TestClass]
    public class FormatConversionTests
    {
        [DataTestMethod]
        [DataRow(ResizingFormat.Jpeg, "jpg")]
        [DataRow(ResizingFormat.Png, "png")]
        [DataRow(ResizingFormat.WebP, "webp")]
        public void Formats_UseProviderSpecificSyntax(ResizingFormat format, string token)
        {
            const string url = "https://img.example.com/a.jpg";
            Assert.AreEqual(url + "?format=" + token, ResizingUtil.FormatImageUrl(url, format));
            Assert.AreEqual(url + "?imageMogr2/format/" + token, ResizingUtil.FormatTencentCdnUrl(url, format));
            Assert.AreEqual(url + "?x-oss-process=image/format," + token, ResizingUtil.FormatAliyunCdnUrl(url, format));
        }

        [DataTestMethod]
        [DataRow(ResizingMode.Crop, "!200x300r/gravity/center/crop/200x300", "fill")]
        [DataRow(ResizingMode.Max, "200x300", "lfit")]
        [DataRow(ResizingMode.Pad, "200x300/pad/1", "pad")]
        public void ResizeAndWebP_ComposeOperations(ResizingMode mode, string tencent, string aliyun)
        {
            const string url = "https://img.example.com/a.jpg?v=1#preview";
            Assert.AreEqual("https://img.example.com/a.jpg?v=1&imageMogr2/thumbnail/" + tencent + "/format/webp#preview",
                ResizingUtil.FormatTencentCdnUrl(url, 200, 300, ResizingFormat.WebP, mode));
            Assert.AreEqual("https://img.example.com/a.jpg?v=1&x-oss-process=image/resize,m_" + aliyun + ",w_200,h_300/format,webp#preview",
                ResizingUtil.FormatAliyunCdnUrl(url, 200, 300, ResizingFormat.WebP, mode));
        }

        [TestMethod]
        public void ServerFormat_KeepsSourceExtensionAndAlias()
        {
            ResizingUtil.Host = "https://img.example.com";
            const string template = "/u/face/a{0}x{1}{2}.png?v=1#preview";
            const string expected = "https://img.example.com/u/face/a200x300p.png?v=1&format=webp#preview";
            Assert.AreEqual(expected, ResizingUtil.FormatUrl(template, 200, 300, ResizingFormat.WebP, ResizingMode.Pad));
            Assert.AreEqual(expected, ResizingUtil.Format(template, 200, 300, ResizingFormat.WebP, ResizingMode.Pad));
            Assert.AreEqual(ResizingUtil.FormatUrl(template, 200, 300),
                ResizingUtil.FormatUrl(template, 200, 300, ResizingFormat.Original));
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("https://img.example.com/a.png?v=1#preview")]
        public void OriginalFormat_DoesNotAddConversion(string url)
        {
            Assert.AreEqual(url, ResizingUtil.FormatImageUrl(url, ResizingFormat.Original));
            Assert.AreEqual(url, ResizingUtil.FormatTencentCdnUrl(url, ResizingFormat.Original));
            Assert.AreEqual(url, ResizingUtil.FormatAliyunCdnUrl(url, ResizingFormat.Original));
        }

        [DataTestMethod]
        [DataRow("?", "?")]
        [DataRow("?v=1&", "?v=1&")]
        [DataRow("", "?")]
        [DataRow("?v=1", "?v=1&")]
        public void FormatQuery_PreservesFragmentAndSeparators(string existing, string separator)
        {
            const string url = "https://img.example.com/a.jpg";
            Assert.AreEqual(url + separator + "format=webp#preview",
                ResizingUtil.FormatImageUrl(url + existing + "#preview", ResizingFormat.WebP));
            Assert.AreEqual(url + separator + "imageMogr2/format/webp#preview",
                ResizingUtil.FormatTencentCdnUrl(url + existing + "#preview", ResizingFormat.WebP));
            Assert.AreEqual(url + separator + "x-oss-process=image/format,webp#preview",
                ResizingUtil.FormatAliyunCdnUrl(url + existing + "#preview", ResizingFormat.WebP));
        }

        [DataTestMethod]
        [DataRow(ResizingMode.Crop, "cover")]
        [DataRow(ResizingMode.Max, "inside")]
        [DataRow(ResizingMode.Pad, "contain")]
        public void Aws_EncodesRealHandlerRequest(ResizingMode mode, string fit)
        {
            const string endpoint = "https://images.example.com/handler/";
            const string key = "图片/a b+?#\".jpg";
            var url = ResizingUtil.FormatAwsCdnUrl(endpoint, "source-bucket", key, 200, 300, ResizingFormat.WebP, mode);
            StringAssert.StartsWith(url, endpoint);
            var request = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(url.Substring(endpoint.Length))));
            Assert.AreEqual("source-bucket", (string)request["bucket"]);
            Assert.AreEqual(key, (string)request["key"]);
            Assert.AreEqual(200, (int)request["edits"]["resize"]["width"]);
            Assert.AreEqual(300, (int)request["edits"]["resize"]["height"]);
            Assert.AreEqual(fit, (string)request["edits"]["resize"]["fit"]);
            Assert.AreEqual("webp", (string)request["edits"]["toFormat"]);
            Assert.IsFalse(url.Contains("x-amz-process"));
        }

        [DataTestMethod]
        [DataRow(ResizingFormat.Original, null)]
        [DataRow(ResizingFormat.Jpeg, "jpeg")]
        [DataRow(ResizingFormat.Png, "png")]
        [DataRow(ResizingFormat.WebP, "webp")]
        public void Aws_FormatOnly_DoesNotResize(ResizingFormat format, string expected)
        {
            const string endpoint = "https://images.example.com/";
            var url = ResizingUtil.FormatAwsCdnUrl(endpoint, "bucket", "a.png", format);
            var request = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(url.Substring(endpoint.Length))));
            Assert.IsNull(request["edits"]["resize"]);
            Assert.AreEqual(expected, (string)request["edits"]["toFormat"]);
        }

        [DataTestMethod]
        [DataRow("relative")]
        [DataRow("ftp://example.com")]
        [DataRow("https://example.com/?signature=value")]
        [DataRow("https://example.com/#fragment")]
        [DataRow("https://user@example.com/")]
        public void Aws_RejectsInvalidEndpoints(string endpoint)
        {
            Assert.ThrowsException<ArgumentException>(() =>
                ResizingUtil.FormatAwsCdnUrl(endpoint, "bucket", "a.jpg", ResizingFormat.WebP));
        }

        [TestMethod]
        public void InvalidArguments_AreRejected()
        {
            const string url = "https://images.example.com/";
            var invalid = (ResizingFormat)999;
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => ResizingUtil.FormatImageUrl(url, invalid));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => ResizingUtil.FormatTencentCdnUrl(url, invalid));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => ResizingUtil.FormatAliyunCdnUrl(url, invalid));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => ResizingUtil.FormatAwsCdnUrl(url, "bucket", "a.jpg", invalid));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => ResizingUtil.FormatTencentCdnUrl(url, 0, 100));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => ResizingUtil.FormatAliyunCdnUrl(url, 100, -1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => ResizingUtil.FormatAwsCdnUrl(url, "bucket", "a.jpg", -1, 100));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => ResizingUtil.FormatTencentCdnUrl(url, 100, 100, (ResizingMode)999));
            Assert.ThrowsException<ArgumentException>(() => ResizingUtil.FormatAwsCdnUrl(url, "", "a.jpg", ResizingFormat.WebP));
            Assert.ThrowsException<ArgumentException>(() => ResizingUtil.FormatAwsCdnUrl(url, "bucket", "", ResizingFormat.WebP));
        }
    }
}
