#!/bin/bash

# v1.0.0-alpha.1 hans:123456@192.168.2.248
set -euo pipefail

if [ $# -eq 0 ]; then
    echo "错误: 请提供版本名称作为参数，例如: v1.0.0-alpha.1"
    exit 1
fi



function setenv() {
    VERSION=$2
    REMOTE=$3
    USERPASS="${3%%@*}"
    HOSTPORT="${3##*@}"
    USER="${USERPASS%%:*}"
    PASS="${USERPASS#*:}"

    WORKDIR=/home/${username}/.flori/build/

    # 检查主机部分是否包含端口
    if [[ "$HOSTPORT" == *:* ]]; then
        HOST="${HOSTPORT%:*}"
        PORT="${HOSTPORT##*:}"
    else
        HOST="$HOSTPORT"
        PORT=""
    fi

}

function tag() {
    git tag $VERSION
}


function build(){
    # 编译文件
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
}

# 上传文件
function upload(){
    BINS=(`ls publish | grep -vE ".pdb|.dbg" | grep "$VERSION"`)
    echo ${BINS[@]}
    REL=""
    for bin in ${BINS[*]}
    do
        REL+="publish/$bin "
    done
    if [[ -d "./publish" && ! ${#BINS} -eq 0 ]]; then
        echo 上传文件
        ./gh release create $VERSION  --title "$VERSION" --notes "Initial release with executable"
        ./gh release upload  $VERSION --clobber $REL  
        echo 上传完成
    fi
}

# publish.sh build v1.0.0-alpha.1 hans:123456@
if [ "$1" = "build"  ]; then
    setenv
    tag
    build
elif [ "$1" = "upload" ]; then
    setenv
    upload

fi




 




