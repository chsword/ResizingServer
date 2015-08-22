namespace ResizingClient
{
    public class ResizingUtil
    {
        public static string Host = System.Configuration.ConfigurationManager.AppSettings["ResizingServer.Host"];
        public string FormatUrl(string format, int width, int height, ResizingMode mode = ResizingMode.Crop)
        {
            return
                $"{Host}{string.Format(format, width, height, GetMode(mode))}";

        }

        private string GetMode(ResizingMode mode)
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