# Agent.Cli - 服务端数据采集上报工具

## 介绍
- 配置: 修改执行文件的 `.env` 文件进行配置
- 数据：采集服务器运行数据
- 上报：数据上传到飞书多维表格
- 域名: 自动更下或添加指定域名的A记录

## 编译
> - linux 使用 ubuntu 22.04 
> - 安装 .NET 8 SDK https://learn.microsoft.com/zh-cn/dotnet/core/install/linux-ubuntu-install?tabs=dotnet8&pivots=os-linux-ubuntu-2204
> - 安装 AOT 编译器 https://learn.microsoft.com/zh-cn/dotnet/core/deploying/native-aot/?tabs=linux-ubuntu%2Cnet8

在 windows git bash 中运行以下命令编译，在`build` 文件夹中自动生成发布文件</br>
在`.env`中指定`DEV_SSH`为`hans:123456@192.168.2.248` 指定linux主机的用户名、密码、IP地址和SSH端口，
会自动打标签，并编译文件，可以执行文件在`build`中可以找到</br>
版本名形如`v1.1.0-alpha.1`,`v1.1.0-alpha.2`,`...`
```bash
# 进入 bash 
# & "C:\Program Files\Git\bin\bash.exe" 
cd agent
bash script/dev.sh build
```


## 发布
运行 `bash script/dev.sh upload`</br>
会获取当前最新tag,创建release, 并发布可执行文件
```bash
# git ls-remote --tags
# git tag -d v1.0.0-alpha.1
# git push origin :refs/tags/v1.0.0-alpha.1
# git tag v1.0.0-alpha.1
# git push origin v1.0.0-alpha.1

bash script/dev.sh upload
```





## 部署
当前版本使用 systemd 管理应用程序，使用`deploy.sh`部署服务，使用以下命令安装
```bash
 bash <(curl -Ls https://raw.githubusercontent.com/harmful-chan/agent/refs/heads/develop/Script/deploy.sh) v1.0.0-alpha.1
```
然后，在`/opt/flori/agent/.env`中设置必要的环境变量</br>
```
# .env
# v1.0.0
AWS_ACCESS_KEY_ID= # AWS Access Key ID
AWS_SECRET_ACCESS_KEY= # AWS Secret Access Key
AWS_DEFAULT_REGION=ap-east-1

# 应用ID SECRET
FEISHU_APP_ID= 
FEISHU_APP_SECRET=
# 飞书 多维表格ID, 表格ID, 表格视图ID
FEISHU_BITTABLE_ID=
FEISHU_BITTABLE_TABLE_ID=
FEISHU_BITTABLE_TABLE_VIEW_ID=

# IPINFO TOKEN 用来获取 IP地址信息
IPINFO_TOKEN=
# 设定的域名, 可空
DEV_DOMAIN=test.bn.fc.fb.dev.153246.com
```

以下是常用命令
```bash
# 重新加载systemd配置
sudo systemctl daemon-reload

# 启动服务
sudo systemctl enable|disable|start|stop|status agent-cli.service

# 查看日志
sudo journalctl -u agent-cli.service -f
```