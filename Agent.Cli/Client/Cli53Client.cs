using DnsZone;
using DnsZone.Records;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Agent.ConsoleApp.Client
{
    public class Cli53Client
    {
        public async Task<string> UpsetIpByDomainAsync(string domain, string ip)
        {
            string main = "153246.com";

            if (domain == null || ip == null)
            {
                throw new ArgumentNullException("域名或IP地址不能为空");
            }

            if (!domain.EndsWith(main))
            {
                throw new ArgumentNullException("域名不为 153246.com");
            }

            (string output, string error) = await RunAsync($"export {main}");
            var zone = DnsZoneFile.Parse(output);
            var record = zone.Records.Where(r => r.Name == domain && r.Type == ResourceRecordType.A).FirstOrDefault();

            string sub = domain.Replace($".{main}", "");
            if (record == null)
            {
                Console.WriteLine($"Cli53Client Add {domain} 60 A {ip}");
                (output, error) = await RunAsync($"rrcreate {main} \"{sub} 60 A {ip}\"");
                return output;
            }
            else
            {
                var aRecord = record as AResourceRecord; // 确保是 A 记录    
                if (aRecord.Name.Equals(domain) && aRecord.Address.ToString().Equals(ip))
                {
                    return $"Cli53Client {aRecord.Name} {aRecord.Address} 没变化";
                }

                Console.WriteLine($"Cli53Client Update {domain} 60 A {ip}");
                (output, error) = await RunAsync($"rrcreate --replace {main} \"{sub} 60 A {ip}\"");
                return output;
            }
        }

        private static Task<(string, string)> RunAsync(string arg)
        {
            return Task.Run(() => {


                string resName, fileName, name = string.Empty;

                

                
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    name = "cli53-linux-amd64";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    name = "cli53-windows-amd64.exe";
                }
                string tempDir = AppDomain.CurrentDomain.BaseDirectory;
                resName = "Agent.Cli.Resources." + name;
                fileName = Path.Combine(tempDir, name);
            
                if (!File.Exists(fileName))
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    using Stream? stream = assembly.GetManifestResourceStream(resName);
                    if (stream == null)
                        throw new InvalidOperationException($"无法找到资源: {resName}");

                    using FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                    stream.CopyTo(fileStream);
                    Console.WriteLine($"Cli53Client Create {fileName}");
                }



                // 设置进程信息
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arg,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = tempDir,
                };

                // 启动进程
                Console.WriteLine($"{startInfo.FileName} {startInfo.Arguments}");
                using Process process = new Process { StartInfo = startInfo };
                process.Start();

                // 读取输出
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();
                
                return (output, error);
            });
        }
    }
}
