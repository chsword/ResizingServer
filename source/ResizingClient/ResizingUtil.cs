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
        public static string Host = System.Configuration.ConfigurationManager.AppSettings["ResizingServer.Host"];
        public static string ApiKey = System.Configuration.ConfigurationManager.AppSettings["ResizingServer.ApiKey"];
        
        public static string UploadUrl = System.Configuration.ConfigurationManager.AppSettings["ResizingServer.UploadUrl"];

        public static string FormatUrl(string format, int width, int height, ResizingMode mode = ResizingMode.Crop)
        {
            return
                $"{Host}{string.Format(format, width, height, GetMode(mode))}";
        }

        public static Task<UploadResult> Upload(Stream stream, string filename, string category)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int) stream.Length);
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