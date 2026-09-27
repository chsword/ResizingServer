#if NET472
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Xml;
using ImageResizer;
using ImageResizer.Configuration;
using Imazen.WebP;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ResizingClient.Tests
{
    [TestClass]
    public class ServerFormatTests
    {
        [DataTestMethod]
        [DataRow("jpg")]
        [DataRow("png")]
        public void WebP_EncodesOriginalSourceAndResizes(string sourceFormat)
        {
            var config = CreateConfig();
            var settings = new ResizeSettings("width=8&height=6&mode=crop&format=webp&quality=70");
            Assert.AreEqual("image/webp", config.Plugins.GetEncoder(settings, "source." + sourceFormat).MimeType);
            Assert.AreEqual(11, typeof(SimpleEncoder).Assembly.GetName().Version.Major);
            Assert.IsTrue(new Version(SimpleEncoder.GetEncoderVersion()) >= new Version(1, 6, 0));

            using (var source = CreateSource(sourceFormat))
            using (var output = new MemoryStream())
            {
                config.CurrentImageBuilder.Build(source, output, settings);
                var bytes = output.ToArray();
                Assert.AreEqual("RIFF", Encoding.ASCII.GetString(bytes, 0, 4));
                Assert.AreEqual("WEBP", Encoding.ASCII.GetString(bytes, 8, 4));
                using (var decoded = new SimpleDecoder().DecodeFromBytes(bytes, bytes.LongLength))
                {
                    Assert.AreEqual(8, decoded.Width);
                    Assert.AreEqual(6, decoded.Height);
                }
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void WebP_PreservesPngTransparency(bool lossless)
        {
            using (var bitmap = new Bitmap(8, 6))
            using (var source = new MemoryStream())
            using (var output = new MemoryStream())
            {
                bitmap.SetPixel(4, 3, Color.FromArgb(120, 200, 80, 20));
                bitmap.Save(source, ImageFormat.Png);
                source.Position = 0;
                CreateConfig().CurrentImageBuilder.Build(source, output,
                    new ResizeSettings("format=webp&lossless=" + lossless));
                var bytes = output.ToArray();
                using (var decoded = new SimpleDecoder().DecodeFromBytes(bytes, bytes.LongLength))
                {
                    Assert.AreEqual(0, (int)decoded.GetPixel(0, 0).A);
                    Assert.AreEqual(120, (int)decoded.GetPixel(4, 3).A);
                }
            }
        }

        [DataTestMethod]
        [DataRow("jpg", "png", "image/png")]
        [DataRow("png", "jpg", "image/jpeg")]
        public void StandardFormats_StillConvertAndKeepTheirDefaults(string sourceFormat, string outputFormat, string mimeType)
        {
            var config = CreateConfig();
            var settings = new ResizeSettings("width=8&height=6&mode=crop&format=" + outputFormat);
            Assert.AreEqual(mimeType, config.Plugins.GetEncoder(settings, "source." + sourceFormat).MimeType);
            Assert.AreEqual(sourceFormat == "jpg" ? "image/jpeg" : "image/png",
                config.Plugins.GetEncoder(new ResizeSettings(), "source." + sourceFormat).MimeType);
            using (var source = CreateSource(sourceFormat))
            using (var output = new MemoryStream())
            {
                config.CurrentImageBuilder.Build(source, output, settings);
                output.Position = 0;
                using (var decoded = Image.FromStream(output))
                {
                    Assert.AreEqual(outputFormat == "jpg" ? ImageFormat.Jpeg.Guid : ImageFormat.Png.Guid, decoded.RawFormat.Guid);
                    Assert.AreEqual(8, decoded.Width);
                    Assert.AreEqual(6, decoded.Height);
                }
            }
        }

        [DataTestMethod]
        [DataRow("win-x86")]
        [DataRow("win-x64")]
        public void NativeRuntime_DeploysAllDependencies(string runtime)
        {
            foreach (var file in new[] { "libwebp.dll", "libsharpyuv.dll", "libwebpdemux.dll", "libwebpmux.dll" })
            {
                var path = Path.Combine(Path.GetDirectoryName(typeof(ServerFormatTests).Assembly.Location),
                    "runtimes", runtime, "native", file);
                Assert.IsTrue(File.Exists(path), "Missing deployed native dependency: " + path);
            }
        }

        private static Config CreateConfig()
        {
            var document = new XmlDocument();
            document.Load(Path.Combine(Path.GetDirectoryName(typeof(ServerFormatTests).Assembly.Location),
                "ResizingServer.Web.config"));
            Assert.IsNotNull(document.SelectSingleNode("/configuration/resizer/plugins/add[@name='WebPEncoder']"));
            // DiskCache requires IIS hosting; the configured encoder pipeline can run independently.
            var diskCache = document.SelectSingleNode("/configuration/resizer/plugins/add[@name='DiskCache']");
            if (diskCache != null)
                diskCache.ParentNode.RemoveChild(diskCache);
            return new Config(new ResizerSection(document.SelectSingleNode("/configuration/resizer").OuterXml));
        }

        private static MemoryStream CreateSource(string format)
        {
            var source = new MemoryStream();
            using (var bitmap = new Bitmap(16, 12))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.CornflowerBlue);
                bitmap.Save(source, format == "jpg" ? ImageFormat.Jpeg : ImageFormat.Png);
            }
            source.Position = 0;
            return source;
        }
    }
}
#endif
