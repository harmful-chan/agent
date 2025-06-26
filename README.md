# Agent.Cli - 服务端数据采集上报工具

## 介绍
- 配置: 修改执行文件的 `.env` 文件进行配置
- 数据：采集服务器运行数据
- 上报：数据上传到飞书多维表格
- 域名: 自动更下或添加指定域名的A记录


## 编译
```bash

// linux 使用 ubuntu 22.04 运行
// 安装 .NET 8 SDK https://learn.microsoft.com/zh-cn/dotnet/core/install/linux-ubuntu-install?tabs=dotnet8&pivots=os-linux-ubuntu-2204
// 安装 AOT 编译器 https://learn.microsoft.com/zh-cn/dotnet/core/deploying/native-aot/?tabs=linux-ubuntu%2Cnet8
ssh hans@192.168.2.248 "cd agent && dotnet publish -r linux-x64 -c Release --self-contained true -p:PublishAot=true -p:AssemblyName=agent-cli-v1.0.0-alpha.1-linux-x64"

// windows 使用git bash 运行
dotnet publish -r win-x64 -c Release --self-contained true -p:PublishAot=true -p:AssemblyName=agent-cli-v1.0.0-alpha.1-win-x64  
// 复制 linux 生成的文件
scp -r hans@192.168.2.248:/home/hans/agent/Agent.Cli/bin/Release/net8.0/linux-x64/publish Agent.Cli/bin/Release/net8.0/linux-x64
```

## 发布
```bash
// 可执行文件 发布到 tag 下
// Agent.Cli/bin/Release/net8.0/linux-x64/agent-cli-v1.0.0-alpha.1-linux-x64
// Agent.Cli/bin/Release/net8.0/win-x64/agent-cli-v1.0.0-alpha.1-win-x64.exe


# git ls-remote --tags
# git tag -d v1.0.0-alpha.1
# git push origin :refs/tags/v1.0.0-alpha.1

git tag v1.0.0-alpha.1
git push origin v1.0.0-alpha.1
```

## 用法
新部署服务器直接运行 `./agent` 即可
```bash
// 查看会话
screen -ls
// 创建会话 
screen -S my_screen
// 进入会话 
screen -r my_screen
// 退出会话
screen -d my_screen
// 删除会话
screen -S my_screen -X quit

vi .env # 修改配置
./agent-linux-amd64 // linux
./agent-windows-amd64.exe // windows
```