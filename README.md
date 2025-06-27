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
> - linux 使用 ubuntu 22.04 运行
> - 安装 .NET 8 SDK https://learn.microsoft.com/zh-cn/dotnet/core/install/linux-ubuntu-install?tabs=dotnet8&pivots=os-linux-ubuntu-2204
>-  安装 AOT 编译器 https://learn.microsoft.com/zh-cn/dotnet/core/deploying/native-aot/?tabs=linux-ubuntu%2Cnet8

在 windows git bash 中运行以下命令编译，在`publish` 文件夹中自动生成发布文件</br>
其中`hans:123456@192.168.2.248` 指定linux主机的用户名、密码、IP地址和SSH端口，
```bash
# 进入 bash 
# & "C:\Program Files\Git\bin\bash.exe" 
cd Script
bash publish.sh v1.0.0-alpha.1 hans:123456@192.168.2.248
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