
using Agent.ConsoleApp.Client;
using DotEnv.Core;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Agent.Cli
{


    public class Program
    {
        static ILogger? _logger;
        class RunOutput : TextWriter
        {
            public override Encoding Encoding => Encoding.Unicode;
            public override void WriteLine(string? value)
            {
                _logger?.LogInformation(value);
            }
        }

        static async Task Main(string[] args)
        {


            #region 日志

            using ILoggerFactory factory = LoggerFactory.Create(
                builder => builder.AddSimpleConsole(options =>
                {

                    //options.IncludeScopes = false;
                    options.SingleLine = true;
                    //options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                }));
            _logger = factory.CreateLogger("");

            Console.SetOut(new RunOutput());
            #endregion


            #region 加载环境变量
            new EnvLoader().Load();
            var envs = new List<string>() {
                "AWS_ACCESS_KEY_ID",
                "AWS_SECRET_ACCESS_KEY",
                "AWS_DEFAULT_REGION",
                "FEISHU_BITTABLE_ID",
                "FEISHU_BITTABLE_TABLE_ID",
                "FEISHU_BITTABLE_TABLE_VIEW_ID",
                "FEISHU_APP_ID",
                "FEISHU_APP_SECRET",
                "IPINFO_TOKEN",
                "DEV_DOMAIN"
            };
            foreach (var env in envs)
            {
                if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(env)))
                {
                    Console.WriteLine($"{env} 不存在");
                    return;
                }
            }
            #endregion

            while (true)
            {
                try
                {
                    // 获取服务信息
                    Console.WriteLine("获取本机信息");
                    var serverClient = new ServerClient();
                    string? ipinfotoken = Environment.GetEnvironmentVariable("IPINFO_TOKEN") ?? "";
                    await serverClient.GetLocalInfo(ipinfotoken);
                    Console.WriteLine(serverClient.ToJson());

                    // 上传飞书
                    Console.WriteLine("上传数据");
                    var feishuclient = new FeishuClient();
                    string id = Environment.GetEnvironmentVariable("FEISHU_APP_ID") ?? "";
                    string key = Environment.GetEnvironmentVariable("FEISHU_APP_SECRET") ?? "";
                    string token = await feishuclient.GetFeishuTokenAsync(id, key) ?? "";
                    string bitId = Environment.GetEnvironmentVariable("FEISHU_BITTABLE_ID") ?? "";
                    string bitTableId = Environment.GetEnvironmentVariable("FEISHU_BITTABLE_TABLE_ID") ?? "";
                    string bitTableViewId = Environment.GetEnvironmentVariable("FEISHU_BITTABLE_TABLE_VIEW_ID") ?? "";
                    await feishuclient.UploadFeishuServerStatus(serverClient.Domain ?? "", serverClient.IPAddress ?? "", 
                        serverClient.ToJson(), token, bitId, bitTableId, bitTableViewId);

                    // 更新域名
                    Console.WriteLine("上传域名");
                    var cli53 = new Cli53Client();
                    string domain = Environment.GetEnvironmentVariable("DEV_DOMAIN") ?? "";
                    string address = serverClient.IPAddress ?? "";
                    string ret = await cli53.UpsetIpByDomainAsync(domain, address);
                    Console.WriteLine(ret);
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    Console.WriteLine("wait 30s");
                    await Task.Delay(30000);
                }



            }



        }

    }

}
