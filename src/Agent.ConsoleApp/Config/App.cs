using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.ConsoleApp.Config
{
    public class App
    {
        [EnvKey("AWS_ACCESS_KEY_ID")]
        public string? AwsAccessKeyId { get; set; }

        [EnvKey("AWS_SECRET_ACCESS_KEY")]
        public string? AwsSecretAccessKey { get; set; }

        [EnvKey("AWS_DEFAULT_REGION")]
        public string? AwsDefaultRegion { get; set; }

        [EnvKey("FEISHU_SECRET_ID")]
        public string? FershuSecretId { get; set; }

        [EnvKey("FEISHU_SECRET_KEY")]
        public string? FeishuSecretKey { get; set; }

    }
}
