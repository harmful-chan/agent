#!/bin/bash


set -euo pipefail

if [ -f "/etc/systemd/system/agent-cli.service" ]; then
	echo "服务已存在"
	exit 1
if

if [ $# -eq 0 ]; then
    echo "错误: 请提供版本名称作为参数，例如: v1.0.0-alpha.1"
    exit 1
fi
VERSION=$1


mkdir -p /opt/agent-cli
wget  https://github.com/harmful-chan/agent/releases/download/v1.0.0-alpha.1/agent-cli-v1.0.0-alpha.1-linux-x64 -o /opt/agent-cli/agent-cli-v1.0.0-alpha.1-linux-x64
ln -s /opt/agent-cli/agent-cli-v1.0.0-alpha.1-linux-x64 /opt/agent-cli/agent-cli
# 写入服务进程
sudo cat >/etc/systemd/system/agent-cli.service <<EOF
[Unit]
Description=Agent Cli Service for .NET 8 Console Application
After=network.target
Requires=network.target

[Service]
# 应用程序的用户和组
User=root
Group=root

# 应用程序的工作目录和执行路径
WorkingDirectory=/opt/agent-cli/
ExecStart=/opt/agent-cli/agent-cli

# 进程管理选项
Restart=on-failure
RestartSec=5s
KillMode=process
TimeoutStopSec=30

# 环境变量
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_ROOT=/usr/share/dotnet

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl daemon-reload
# 尾行