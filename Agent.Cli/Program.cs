
using Agent.ConsoleApp.Client;
using Agent.ConsoleApp.Model;
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

                    options.IncludeScopes = false;
                    options.SingleLine = true;
                    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                }));
            _logger = factory.CreateLogger("log");

            Console.SetOut(new RunOutput());
            #endregion


            #region 加载环境变量
            new EnvLoader().Load();
            var envs = new List<string>() {
                "AWS_ACCESS_KEY_ID",
                "AWS_SECRET_ACCESS_KEY",
                "AWS_DEFAULT_REGION",
                "FEISHU_SECRET_ID",
                "FEISHU_SECRET_KEY",
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
                    var status = new ServerStatus();
                    string? ipinfotoken = Environment.GetEnvironmentVariable("IPINFO_TOKEN") ?? "";
                    await status.GetLocalInfo(ipinfotoken);
                    Console.WriteLine(status.ToJson());

                    // 上传飞书
                    var feishuclient = new FeishuClient();
                    string id = Environment.GetEnvironmentVariable("FEISHU_APP_ID") ?? "";
                    string key = Environment.GetEnvironmentVariable("FEISHU_APP_SECRET") ?? "";
                    string token = await feishuclient.GetFeishuTokenAsync(id, key);
                    await feishuclient.UploadFeishuServerStatus(status, token);

                    // 更新域名
                    var cli53 = new Cli53Client();
                    string domain = Environment.GetEnvironmentVariable("DEV_DOMAIN") ?? "";
                    string address = status.IPAddress ?? "";
                    string ret = await cli53.UpsetIpByDomainAsync(domain, address);
                    Console.WriteLine(ret);
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    await Task.Delay(30000);
                }



            }



        }

    }

}
