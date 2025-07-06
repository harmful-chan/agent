
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Agent.ConsoleApp.Client
{
    public class FeishuClient
    {
        public void AssertJson(string? content)
        {
            if (content == null || JsonNode.Parse(content)?["code"]?.ToString().Equals("0") == false)
            {
                throw new Exception($"FeishuClient API调用失败: {content}");
            }
        }

        public async Task<string?> GetFeishuTokenAsync(string appId, string appSecret)
        {
            if(string.IsNullOrWhiteSpace(appId) || string.IsNullOrEmpty(appSecret))
            {
                throw new ArgumentNullException("飞书应用ID或密钥不能为空");
            }

            var client = new RestClient("https://open.feishu.cn/open-apis/auth/v3/tenant_access_token/internal");
            var req = new RestRequest() { Method = Method.Post };
            req.AddHeader("Content-Type", "application/json");
            var body = "{\"app_id\":\""+appId+"\",\"app_secret\":\""+appSecret+"\"}";
            req.AddParameter("application/json", body, ParameterType.RequestBody);
            var res = await client.ExecuteAsync(req);
            AssertJson(res.Content);

            var token = JsonNode.Parse(res.Content ?? "{}")?["tenant_access_token"]?.ToString();
            return token;
        }

        // 上传到飞书多维表格（使用 RestClient/RestRequest 改写）
        public async Task UploadServerStatusById(string id, string statusJson, string token, string bitId, string tableId, string viewId)
        {
            var client = new RestClient("https://open.feishu.cn");
            
            // 查询记录接口
            var searchRequest = new RestRequest($"/open-apis/bitable/v1/apps/{bitId}/tables/{tableId}/records/search", Method.Post);
            searchRequest.AddHeader("Authorization", $"Bearer {token}");
            searchRequest.AddHeader("Content-Type", "application/json");
            var searchBody = "{" +
                "\"automatic_fields\":false," +
                "\"field_names\":[\"ID\"]," +
                "\"filter\":{" +
                    "\"conditions\":[" +
                        "{\"field_name\":\"ID\",\"operator\":\"is\",\"value\":[\"" + id + "\"]}" +
                    "]," +
                    "\"conjunction\":\"and\"" +
                "},\"sort\":[{\"desc\":true,\"field_name\":\"ID\"}],\"view_id\":\""+viewId+"\"}";
            searchRequest.AddParameter("application/json", searchBody, ParameterType.RequestBody);
            var searchResponse = await client.ExecuteAsync(searchRequest);
            AssertJson(searchResponse.Content);
        

            var searchJson = JsonNode.Parse(searchResponse.Content ?? "{}");

            // 更新或创建记录  
            int total = int.Parse(searchJson?["data"]?["total"]?.ToString() ?? "0");
            Console.WriteLine($"FeishuClient {id} 数量 {total}");
            RestRequest? req = null;
            if (total > 0)
            {
                Console.WriteLine($"FeishuClient Update {id}");
                var recordId = searchJson?["data"]?["items"]?[0]?["record_id"]?.ToString();
                req = new RestRequest($"/open-apis/bitable/v1/apps/{bitId}/tables/{tableId}/records/{recordId}", Method.Put);
            }
            else
            {
                Console.WriteLine($"FeishuClient Add {id}");
                req = new RestRequest($"/open-apis/bitable/v1/apps/{bitId}/tables/{tableId}/records", Method.Post);
            }
            req.AddHeader("Authorization", $"Bearer {token}");
            req.AddHeader("Content-Type", "application/json");
            var body = "{\"fields\":" + statusJson + "}";
            req.AddParameter("application/json", body, ParameterType.RequestBody);
            var res = await client.ExecuteAsync(req);
            var str = res.Content;
            AssertJson(res.Content);
        }

    }
}
