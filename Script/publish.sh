#!/bin/bash

# v1.0.0-alpha.1 hans:123456@192.168.2.248
set -euo pipefail

if [ $# -eq 0 ]; then
    echo "错误: 请提供版本名称作为参数，例如: v1.0.0-alpha.1"
    exit 1
fi
VERSION=$1
REMOTE=$2
userpass="${2%%@*}"
hostport="${2##*@}"
username="${userpass%%:*}"
password="${userpass#*:}"

WORKDIR=/home/${username}/.flori/build/
# 检查主机部分是否包含端口
if [[ "$hostport" == *:* ]]; then
    hostname="${hostport%:*}"
    port="${hostport##*:}"
else
    hostname="$hostport"
    port=""
fi

mkdir -p publish
git clone https://github.com/harmful-chan/agent --single-branch $VERSION
./sshpass.exe -p ${password} ssh ${username}@${hostport} "mkdir -p $WORKDIR"
./sshpass.exe -p ${password} scp -r $VERSION ${username}@${hostport}:$WORKDIR

pushd $VERSION
dotnet publish -r win-x64 -c Release --self-contained true -p:PublishAot=true -p:AssemblyName=agent-cli-${VERSION}-win-x64
popd

cp -r $VERSION/Agent.Cli/bin/Release/net8.0/win-x64/publish/* publish
rm -rf $VERSION

./sshpass.exe -p ${password} ssh ${username}@${hostport} "pushd $WORKDIR/$VERSION && dotnet publish -r linux-x64 -c Release --self-contained true -p:PublishAot=true -p:AssemblyName=agent-cli-${VERSION}-linux-x64 && popd"
./sshpass.exe -p ${password} scp -r ${username}@${hostport}:$WORKDIR/$VERSION/Agent.Cli/bin/Release/net8.0/linux-x64/publish/* publish
./sshpass.exe -p ${password} ssh ${username}@${hostport} "rm -rf $WORKDIR/$VERSION"

echo 编译完成



