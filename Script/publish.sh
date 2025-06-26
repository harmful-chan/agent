#!/bin/bash

set -euo pipefail

if [ $# -eq 0 ]; then
    echo "错误: 请提供版本名称作为参数，例如: v1.0.0-alpha.1"
    exit 1
fi

VERSION=$1


echo "[1/4] 编译 linux-x64"
ssh hans@192.168.2.248 "cd agent && git pull && dotnet publish -r linux-x64 -c Release --self-contained true -p:PublishAot=true -p:AssemblyName=agent-cli-${VERSION}-linux-x64"
echo "[2/4] 编译 win-x64"
dotnet publish -r win-x64 -c Release --self-contained true -p:PublishAot=true -p:AssemblyName=agent-cli-${VERSION}-win-x64  
echo "[3/4] 拷贝执行文件"
scp -r hans@192.168.2.248:/home/hans/agent/Agent.Cli/bin/Release/net8.0/linux-x64/publish Agent.Cli/bin/Release/net8.0/linux-x64
echo "[4/4] 发布 tag"
git tag $VERSION
git push origin $VERSION