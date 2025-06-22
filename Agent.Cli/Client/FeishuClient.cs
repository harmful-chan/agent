using Agent.ConsoleApp.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Agent.ConsoleApp.Client
{
    public class FeishuClient
    {
        public async Task<string> GetFeishuTokenAsync(string appId, string appSecret)
        {
            if(string.IsNullOrWhiteSpace(appId) || string.IsNullOrEmpty(appSecret))
            {
                throw new ArgumentNullException("飞书应用ID或密钥不能为空");
            }

            var client = new RestClient("https://open.feishu.cn/open-apis/auth/v3/tenant_access_token/internal");
            var req = new RestRequest() { Method = Method.Post };
            req.AddHeader("Content-Type", "application/json");
            var body = "{\"app_id\":\"cli_a7b7327bb5fd500b\",\"app_secret\":\"DEsMubDWYd7TaQisWD2AFfzwNuxJfRee\"}";
            req.AddParameter("application/json", body, ParameterType.RequestBody);
            var rsp = await client.ExecuteAsync(req);
            var json = rsp.Content != null ? JsonNode.Parse(rsp.Content) : "{}";
            var token = json?["tenant_access_token"]?.ToString();
            return token ?? "";
        }

        // 上传到飞书多维表格（使用 RestClient/RestRequest 改写）
        public async Task UploadFeishuServerStatus(ServerStatus status, string token)
        {
            var client = new RestClient("https://open.feishu.cn");
            // 查询记录接口
            Console.WriteLine("查询记录");
            var searchRequest = new RestRequest("/open-apis/bitable/v1/apps/Y51VbNUf3askQ2sDdm9chN8JnAc/tables/tblWxQzjloN1vU06/records/search", Method.Post);
            searchRequest.AddHeader("Authorization", $"Bearer {token}");
            searchRequest.AddHeader("Content-Type", "application/json");
            var searchBody = "{" +
                "\"automatic_fields\":false," +
                "\"field_names\":[\"域名ID\"]," +
                "\"filter\":{" +
                    "\"conditions\":[" +
                        "{\"field_name\":\"域名ID\",\"operator\":\"is\",\"value\":[\"" + status.Domain + "\"]}" +
                    "]," +
                    "\"conjunction\":\"and\"" +
                "},\"sort\":[{\"desc\":true,\"field_name\":\"域名ID\"}],\"view_id\":\"vewPimz5Tb\"}";
            searchRequest.AddParameter("application/json", searchBody, ParameterType.RequestBody);
            var searchResponse = await client.ExecuteAsync(searchRequest);
            var searchJson = searchResponse.Content != null ? JsonNode.Parse(searchResponse.Content) : "{}";
            ;

            // 更新或创建记录
            int total = int.Parse(searchJson?["data"]?["total"]?.ToString() ?? "0");
            Console.WriteLine($"total {total}");
            RestRequest? req = null;
            if (total > 0)
            {
                Console.WriteLine($"更新记录 {status.Domain} {status.IPAddress}");
                var recordId = searchJson?["data"]?["items"]?[0]?["record_id"]?.ToString();
                req = new RestRequest($"/open-apis/bitable/v1/apps/Y51VbNUf3askQ2sDdm9chN8JnAc/tables/tblWxQzjloN1vU06/records/{recordId}", Method.Put);
            }
            else
            {
                Console.WriteLine($"更新记录 {status.Domain} {status.IPAddress}");
                req = new RestRequest("/open-apis/bitable/v1/apps/Y51VbNUf3askQ2sDdm9chN8JnAc/tables/tblWxQzjloN1vU06/records", Method.Post);
            }
            req.AddHeader("Authorization", $"Bearer {token}");
            req.AddHeader("Content-Type", "application/json");
            var body = "{\"fields\":" + status.ToJson() + "}";
            req.AddParameter("application/json", body, ParameterType.RequestBody);
            var res = await client.ExecuteAsync(req);
            var str = res.Content;
        }

    }
}
