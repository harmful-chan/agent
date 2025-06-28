#!/bin/bash


set -euo pipefail

if [ $# -eq 0 ]; then
    echo "错误: 请提供版本名称作为参数，例如: v1.0.0-alpha.1"
    exit 1
fi

VERSION=$1
RELEASES=agent-cli-${VERSION}-linux-x64
WORKDIR=/opt/flori/agent

sudo mkdir -p $WORKDIR
pushd $WORKDIR

if [ ! -f "$RELEASES" ]; then
	echo "[INFO] 下载 $RELEASES "
	sudo wget  "https://git.floribird.com/https://github.com/harmful-chan/agent/releases/download/${VERSION}/${RELEASES}"
	sudo chmod a+x $RELEASES
	sudo chown root:root $RELEASES
	sudo ln -s $PWD/$RELEASES $PWD/flori-agent
fi


if [ ! -f /etc/systemd/system/flori-agent.service ]; then
	# 写入服务进程
	echo "[INFO] 创建 /etc/systemd/system/flori-agent.service"
	sudo cat >flori-agent.service <<EOF
[Unit]
Description=Agent Cli Service for .NET 8 Console Application
After=network.target
Requires=network.target

[Service]
# 应用程序的用户和组
User=root
Group=root

# 应用程序的工作目录和执行路径
WorkingDirectory=/opt/flori/agent/
ExecStart=/opt/flori/agent/flori-agent

# 进程管理选项
Restart=on-failure
RestartSec=5s
KillMode=process
TimeoutStopSec=30

[Install]
WantedBy=multi-user.target
EOF
	sudo mv flori-agent.service /etc/systemd/system/flori-agent.service
	sudo chown root:root /etc/systemd/system/flori-agent.service
	sudo systemctl daemon-reload
fi

popd




