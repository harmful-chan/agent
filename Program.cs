using Agent.Config;
using DotEnv.Core;

namespace Agent
{
    internal class Program
    {
        static void Main(string[] args)
        {
            new EnvLoader().Load();
            var settings = new EnvBinder().Bind<App>();
            string key1 = settings.AwsAccessKeyId;
            string key2 = settings.AwsSecretAccessKey;
            Console.WriteLine($"{key1},{key2}");
            Console.ReadKey();
        }
    }
}
