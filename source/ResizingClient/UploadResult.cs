using Newtonsoft.Json;

namespace ResizingClient
{
    public class UploadResult
    {
        [JsonProperty("success")]
        public bool IsSuccess { get; set; }
        [JsonProperty("format")]
        public string FormatUrl { get; set; }
        [JsonProperty("raw")]
        public string RawUrl { get; set; }
    }
}