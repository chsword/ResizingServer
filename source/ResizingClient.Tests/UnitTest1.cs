using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ResizingClient.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void FormatUrl_UsesExpectedModeToken()
        {
            ResizingUtil.Host = "https://img.example.com";

            Assert.AreEqual("https://img.example.com/u/face/100x100c.jpg", ResizingUtil.FormatUrl("/u/face/{0}x{1}{2}.jpg", 100, 100, ResizingMode.Crop));
            Assert.AreEqual("https://img.example.com/u/face/100x100m.jpg", ResizingUtil.FormatUrl("/u/face/{0}x{1}{2}.jpg", 100, 100, ResizingMode.Max));
            Assert.AreEqual("https://img.example.com/u/face/100x100p.jpg", ResizingUtil.FormatUrl("/u/face/{0}x{1}{2}.jpg", 100, 100, ResizingMode.Pad));
        }

        [TestMethod]
        public void Format_IsBackwardCompatibleAlias()
        {
            ResizingUtil.Host = "https://img.example.com";

            Assert.AreEqual("https://img.example.com/u/face/100x100p.jpg", ResizingUtil.Format("/u/face/{0}x{1}{2}.jpg", 100, 100, ResizingMode.Pad));
        }

        [TestMethod]
        public void FormatTencentCdnUrl_UsesExpectedQuery()
        {
            Assert.AreEqual(
                "https://img.example.com/u/face/a.jpg?imageMogr2/thumbnail/200x300/gravity/center/crop/200x300",
                ResizingUtil.FormatTencentCdnUrl("https://img.example.com/u/face/a.jpg", 200, 300, ResizingMode.Crop));
            Assert.AreEqual(
                "https://img.example.com/u/face/a.jpg?imageMogr2/thumbnail/200x300",
                ResizingUtil.FormatTencentCdnUrl("https://img.example.com/u/face/a.jpg", 200, 300, ResizingMode.Max));
            Assert.AreEqual(
                "https://img.example.com/u/face/a.jpg?imageMogr2/thumbnail/200x300/pad/1",
                ResizingUtil.FormatTencentCdnUrl("https://img.example.com/u/face/a.jpg", 200, 300, ResizingMode.Pad));
        }

        [TestMethod]
        public void FormatAliyunCdnUrl_UsesExpectedQuery()
        {
            Assert.AreEqual(
                "https://img.example.com/u/face/a.jpg?x-oss-process=image/resize,m_fill,w_200,h_300",
                ResizingUtil.FormatAliyunCdnUrl("https://img.example.com/u/face/a.jpg", 200, 300, ResizingMode.Crop));
            Assert.AreEqual(
                "https://img.example.com/u/face/a.jpg?x-oss-process=image/resize,m_lfit,w_200,h_300",
                ResizingUtil.FormatAliyunCdnUrl("https://img.example.com/u/face/a.jpg", 200, 300, ResizingMode.Max));
            Assert.AreEqual(
                "https://img.example.com/u/face/a.jpg?x-oss-process=image/resize,m_pad,w_200,h_300",
                ResizingUtil.FormatAliyunCdnUrl("https://img.example.com/u/face/a.jpg", 200, 300, ResizingMode.Pad));
        }

        [TestMethod]
        public void FormatAwsCdnUrl_UsesExpectedQuery()
        {
            Assert.AreEqual(
                "https://img.example.com/u/face/a.jpg?x-amz-process=image/resize,w_200,h_300,m_cover",
                ResizingUtil.FormatAwsCdnUrl("https://img.example.com/u/face/a.jpg", 200, 300, ResizingMode.Crop));
            Assert.AreEqual(
                "https://img.example.com/u/face/a.jpg?x-amz-process=image/resize,w_200,h_300,m_fit",
                ResizingUtil.FormatAwsCdnUrl("https://img.example.com/u/face/a.jpg", 200, 300, ResizingMode.Max));
            Assert.AreEqual(
                "https://img.example.com/u/face/a.jpg?x-amz-process=image/resize,w_200,h_300,m_pad",
                ResizingUtil.FormatAwsCdnUrl("https://img.example.com/u/face/a.jpg", 200, 300, ResizingMode.Pad));
        }

        [TestMethod]
        public void CdnUrls_AppendWithAmpersandWhenQueryExists()
        {
            Assert.AreEqual(
                "https://img.example.com/u/face/a.jpg?v=1&x-oss-process=image/resize,m_fill,w_200,h_300",
                ResizingUtil.FormatAliyunCdnUrl("https://img.example.com/u/face/a.jpg?v=1", 200, 300, ResizingMode.Crop));
        }

        [TestMethod]
        public void CdnUrls_InsertQueryBeforeFragment()
        {
            Assert.AreEqual(
                "https://img.example.com/u/face/a.jpg?x-oss-process=image/resize,m_fill,w_200,h_300#preview",
                ResizingUtil.FormatAliyunCdnUrl("https://img.example.com/u/face/a.jpg#preview", 200, 300, ResizingMode.Crop));
        }
    }
}
