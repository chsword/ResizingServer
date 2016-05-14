using System;

namespace ResizingServer.Controllers
{
    public class ResizingPath
    {
        public string Year { get;private set; }
        public string Month { get; private set; }
        public string Date { get; private set; }
        public string Category { get; private set; }
        public string PhysicalPath { get; private set; }
        public string PhysicalFilename { get; private set; }
        public string VirtualFormatFilename { get; private set; }
        public string RawPath { get; set; }
        public ResizingPath(string category,string extension)
        {
            // 0246 guid
            // 0 1 ÈÕ 2 3Äê 4 5ÔÂ 6 7Width 8Height 9Mode 10 ext  +2
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
            RawPath = $"/upload/{category}/{Year}{Month}/{Date}/{guid}{extension}";
            PhysicalFilename = $"{PhysicalPath}/{guid}{extension}";
            VirtualFormatFilename =
                $"/u/{category}/{guid1}{Date}{guid2}{Year}{guid3}{Month}{guid4}{{0}}x{{1}}{{2}}{extension}";
        }
    }
}