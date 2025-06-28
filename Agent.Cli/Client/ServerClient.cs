using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Agent.ConsoleApp.Client
{
    [JsonSerializable(typeof(ServerClient))]
    public partial class ServerStatusJsonContext : JsonSerializerContext
    {
    }

    public class ServerClient
    {

        #region 属性
        [JsonPropertyName("域名ID")]
        public string? Domain { get; set; }
        [JsonPropertyName("IP地址")]
        public string? IPAddress { get; set; }
        [JsonPropertyName("状态")]
        public string? Status { get; set; }
        [JsonPropertyName("区域")]
        public string? Region { get; set; }
        [JsonPropertyName("系统")]
        public string? OS { get; set; }
        [JsonPropertyName("启动时间")]
        public string? BootTime { get; set; }
        [JsonPropertyName("上报时间")]
        public string? ReportTime { get; set; }

        #endregion

        public async Task GetLocalInfo(string ipinfotoken)
        {
            string uri = $"https://ipinfo.io/json?token="+ ipinfotoken;
            Console.WriteLine(uri);
            HttpClientHandler handler = new HttpClientHandler();
            using HttpClient client = new HttpClient(handler);
            var res = await client.GetAsync(uri);
            var str = await res.Content.ReadAsStringAsync();
            var json = JsonNode.Parse(str);
            Console.WriteLine(str.Replace("\n", "").Replace(" ", ""));

            IPAddress = json?["ip"]?.ToString() ?? "unknown";
            Region = $"{json?["country"]}/{json?["region"]}/{json?["city"]}";
            BootTime = DateTime.Now.AddMilliseconds(-Environment.TickCount).ToString("yyyy-MM-dd HH:mm:ss");
            Domain = Environment.GetEnvironmentVariable("DEV_DOMAIN") ?? "unknown";
            Status = !string.IsNullOrWhiteSpace(IPAddress) ? "在线" : "unknown";
            OS = RuntimeInformation.OSDescription;
            ReportTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, ServerStatusJsonContext.Default.ServerClient);
        }
    }
}
