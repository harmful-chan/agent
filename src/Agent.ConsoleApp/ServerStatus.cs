using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Agent.ConsoleApp
{
    public class ServerStatus
    {
        [JsonPropertyName("唯一标识/域名")]
        public string? Domain { get; set; }
        [JsonPropertyName("状态")]
        public string? Status { get; set; }
        [JsonPropertyName("运行时间")]
        public string? Uptime { get; set; }
        [JsonPropertyName("架构")]
        public string? Arch { get; set; }
        [JsonPropertyName("内存")]
        public string? Memory { get; set; }
        [JsonPropertyName("磁盘")]
        public string? Disk { get; set; }
        [JsonPropertyName("区域")]
        public string? Region { get; set; }
        [JsonPropertyName("系统")]
        public string? OS { get; set; }
        [JsonPropertyName("CPU")]
        public string? CPU { get; set; }
        [JsonPropertyName("Load")]
        public string? Load { get; set; }
        [JsonPropertyName("上传")]
        public string? Upload { get; set; }
        [JsonPropertyName("下载")]
        public string? Download { get; set; }
        [JsonPropertyName("启动时间")]
        public string? BootTime { get; set; }
        [JsonPropertyName("上报时间")]
        public string? ReportTime { get; set; }

        public static async Task<ServerStatus> GetServerStatusAsync()
        {
            return await Task.Run(() =>
            {
                var (upload, download) = GetNetworkTraffic();
                var status = new ServerStatus();
                status.Domain = Environment.GetEnvironmentVariable("DEV_DOMAIN") ?? "unknown";
                status.Status = "在线";
                status.Uptime = GetUptime();
                status.Arch = RuntimeInformation.OSArchitecture.ToString();
                status.Memory = GetMemoryInfo();
                status.Disk = GetDiskInfo();
                status.Region = "CN";
                status.OS = RuntimeInformation.OSDescription;
                status.CPU = GetCpuInfo();
                status.Load = GetLoadAvg();
                status.Upload = upload;
                status.Download = download;
                status.BootTime = GetBootTime();
                status.ReportTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                return status;
            });

            
        }

        // 获取系统运行时间
        private static string GetUptime()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    var uptimeText = File.ReadAllText("/proc/uptime").Split(' ')[0];
                    var uptimeSeconds = double.Parse(uptimeText);
                    var ts = TimeSpan.FromSeconds(uptimeSeconds);
                    return $"{(int)ts.TotalHours} 小时 {ts.Minutes} 分钟";
                }
                catch { }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
                    return $"{(int)uptime.TotalHours} 小时 {uptime.Minutes} 分钟";
                }
                catch { }
            }
            return "未知";
        }

        // 获取内存信息
        private static string GetMemoryInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    var lines = File.ReadAllLines("/proc/meminfo");
                    var memTotal = lines.FirstOrDefault(l => l.StartsWith("MemTotal"));
                    if (memTotal != null)
                    {
                        var parts = memTotal.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && double.TryParse(parts[1], out double kb))
                        {
                            var gib = kb / 1024 / 1024;
                            return $"{gib:F2} GiB";
                        }
                    }
                }
                catch { }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    var process = Process.GetCurrentProcess();
                    var bytes = process.WorkingSet64;
                    var mib = bytes / 1024.0 / 1024.0;
                    return $"当前进程占用: {mib:F2} MiB";
                }
                catch { }
            }
            return "未知";
        }

        // 获取磁盘信息（根分区总容量）
        private static string GetDiskInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    var psi = new ProcessStartInfo("df", "-h /")
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    using var proc = Process.Start(psi);
                    var output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();
                    var lines = output.Split('\n');
                    if (lines.Length > 1)
                    {
                        var parts = lines[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 1)
                            return parts[1];
                    }
                }
                catch { }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == Path.GetPathRoot(Environment.SystemDirectory));
                    if (drive != null && drive.IsReady)
                    {
                        var gib = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
                        return $"{gib:F2} GiB";
                    }
                }
                catch { }
            }
            return "未知";
        }

        // 获取CPU信息
        private static string GetCpuInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    var lines = File.ReadAllLines("/proc/cpuinfo");
                    var model = lines.FirstOrDefault(l => l.StartsWith("model name"));
                    if (model != null)
                        return model.Split(':')[1].Trim();
                }
                catch { }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // 方法2：仅返回核心数
                return $"核心数: {Environment.ProcessorCount}";
            }
            return "未知";
        }

        // 获取负载
        private static string GetLoadAvg()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    var load = File.ReadAllText("/proc/loadavg").Split(' ');
                    return $"{load[0]} / {load[1]} / {load[2]}";
                }
                catch { }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    // 确保 PerformanceCounter 正确使用
                    var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    cpuCounter.NextValue();
                    System.Threading.Thread.Sleep(500);
                    var value = cpuCounter.NextValue();
                    return $"{value:F2}%";
                }
                catch { }
            }
            return "未知";
        }

        // 获取启动时间
        private static string GetBootTime()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    var uptimeText = File.ReadAllText("/proc/uptime").Split(' ')[0];
                    var uptimeSeconds = double.Parse(uptimeText);
                    var bootTime = DateTime.Now.AddSeconds(-uptimeSeconds);
                    return bootTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch { }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem WHERE Primary='true'");
                    foreach (var mo in searcher.Get())
                    {
                        var v = mo["LastBootUpTime"]?.ToString();
                        if (!string.IsNullOrEmpty(v))
                        {
                            var dt = ManagementDateTimeConverter.ToDateTime(v);
                            return dt.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    }
                }
                catch { }
            }
            return "未知";
        }

        // 获取网络流量（eth0/ens开头的网卡，Windows返回0）
        private static (string upload, string download) GetNetworkTraffic()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    var lines = File.ReadAllLines("/proc/net/dev");
                    var eth = lines.FirstOrDefault(l => l.Contains("eth0")) ??
                              lines.FirstOrDefault(l => l.Contains("ens"));
                    if (eth != null)
                    {
                        var parts = eth.Split(new[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 9 &&
                            double.TryParse(parts[1], out double rx) &&
                            double.TryParse(parts[9], out double tx))
                        {
                            return ($"{tx / 1024 / 1024:F2} MiB", $"{rx / 1024 / 1024:F2} MiB");
                        }
                    }
                }
                catch { }
            }
            // Windows 下简单返回0
            return ("0 MiB", "0 MiB");
        }
    }
}
