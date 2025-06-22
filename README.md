# Agent.Cli - 服务端数据采集上报工具

## 介绍
- 配置: 修改执行文件的 `.env` 文件进行配置
- 数据：采集服务器运行数据
- 上报：数据上传到飞书多维表格
- 域名: 自动更下或添加指定域名的A记录

## 用法
新部署服务器直接运行 `./agent` 即可
```bash
vi .env # 修改配置

./agent-linux-amd64 // linux
./agent-windows-amd64.exe // windows
```