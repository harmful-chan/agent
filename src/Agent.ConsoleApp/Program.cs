
using Agent.ConsoleApp.Config;
using DotEnv.Core;

namespace Agent.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            new EnvLoader().Load();
            var app = new EnvBinder().Bind<App>();
            Console.WriteLine("Hello, World!");
        }
    }
}
