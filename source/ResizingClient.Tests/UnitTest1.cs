using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ResizingClient.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var result = ResizingUtil.Upload(File.ReadAllBytes("F:\\Book\\战略\\1.pdf"), "1.pdf", "files").Result;
            Console.WriteLine(result.FormatUrl );
            Assert.IsTrue(result.IsSuccess);
        }
    }
}
