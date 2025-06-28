# Agent.Cli - 服务端数据采集上报工具

## 介绍
- 配置: 修改执行文件的 `.env` 文件进行配置
- 数据：采集服务器运行数据
- 上报：数据上传到飞书多维表格
- 域名: 自动更下或添加指定域名的A记录


## 发布
需要发布的版本先打tag，`publish.sh` 脚本根据tag获取代码编译
```bash
# git ls-remote --tags
# git tag -d v1.0.0-alpha.1
# git push origin :refs/tags/v1.0.0-alpha.1

git tag v1.0.0-alpha.1
git push origin v1.0.0-alpha.1
```

## 编译
> - linux 使用 ubuntu 22.04 
> - 安装 .NET 8 SDK https://learn.microsoft.com/zh-cn/dotnet/core/install/linux-ubuntu-install?tabs=dotnet8&pivots=os-linux-ubuntu-2204
> - 安装 AOT 编译器 https://learn.microsoft.com/zh-cn/dotnet/core/deploying/native-aot/?tabs=linux-ubuntu%2Cnet8

在 windows git bash 中运行以下命令编译，在`publish` 文件夹中自动生成发布文件</br>
其中`hans:123456@192.168.2.248` 指定linux主机的用户名、密码、IP地址和SSH端口，
```bash
# 进入 bash 
# & "C:\Program Files\Git\bin\bash.exe" 
cd Script
bash publish.sh v1.0.0-alpha.1 hans:123456@192.168.2.248
```



## 部署
当前版本使用 systemd 管理应用程序，使用`deploy.sh`部署服务，使用以下命令安装
```bash
 bash <(curl -Ls https://raw.githubusercontent.com/harmful-chan/agent/refs/heads/develop/Script/deploy.sh) v1.0.0-alpha.1
```
然后，在`/opt/agent-cli/.env`中设置必要的环境变量</br>

以下是常用命令
```bash
# 重新加载systemd配置
sudo systemctl daemon-reload

# 启动服务
sudo systemctl enable|disable|start|stop|status agent-cli.service

# 查看日志
sudo journalctl -u agent-cli.service -f
```