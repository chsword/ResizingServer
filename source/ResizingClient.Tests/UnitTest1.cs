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
    }
}
