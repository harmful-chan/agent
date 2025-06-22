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
            if(domain == null || ip == null)
            {
                throw new ArgumentNullException("域名或IP地址不能为空");
            }

            string main = "153246.com";
            if (!domain.EndsWith(main))
            {
                return string.Empty; // 只处理 153246.com 域名
            }
            string rawRecord = await RunAsync($"export {main}");
            var zone = DnsZoneFile.Parse(rawRecord);
            var record = zone.Records.Where(r => r.Name == domain && r.Type == ResourceRecordType.A).FirstOrDefault();
            
            string sub = domain.Replace($".{main}", "");
            if (record == null) 
            {
                var ret = await RunAsync($"rrcreate {main} \"{sub} 60 A {ip}\"");
                return ret;

            }
            else
            {
                var aRecord = record as AResourceRecord; // 确保是 A 记录    
                if (aRecord.Name.Equals(domain) && aRecord.Address.ToString().Equals(ip))
                {
                    return $"{aRecord.Name} {aRecord.Address} 没变化";
                }

                var ret = await RunAsync($"rrcreate --replace {main} \"{sub} 60 A {ip}\"");
                return ret;
            }
        }

        private static Task<string> RunAsync(string arg)
        {
            return Task.Run(() => {


                string name = string.Empty;

                // 从资源中提取程序到临时目录
                string tempDir = Path.Combine(Path.GetTempPath(), "EmbeddedTool");
                Directory.CreateDirectory(tempDir);



                Assembly assembly = Assembly.GetExecutingAssembly();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    name = assembly.GetName().Name?.ToString() + ".Resources.cli53-linux-amd64";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    name = assembly.GetName().Name?.ToString() + ".Resources.cli53-windows-amd64.exe";
                }

                string bin = Path.Combine(tempDir, name);
                if (!File.Exists(bin))
                {
                    using Stream? stream = assembly.GetManifestResourceStream(name);
                    if (stream == null)
                        throw new InvalidOperationException($"无法找到资源: {name}");

                    using FileStream fileStream = new FileStream(bin, FileMode.Create, FileAccess.Write);
                    stream.CopyTo(fileStream);
                }



                // 设置进程信息
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = bin,
                    Arguments = arg,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = tempDir,
                };

                // 启动进程
                Console.WriteLine($"执行命令: {startInfo.FileName} {startInfo.Arguments}");
                using Process process = new Process { StartInfo = startInfo };
                process.Start();

                // 读取输出
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();
                Console.WriteLine($"标准输出:{output}");
                Console.WriteLine($"标准错误:{error}");

                return !string.IsNullOrWhiteSpace(output) ? output : error;

            });
        }
    }
}
