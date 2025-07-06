#!/bin/bash

# v1.0.0-alpha.1 hans:123456@192.168.2.248
set -e


function setenv() {
    source $(dirname $0)/.env 
    USERPASS="${DEV_SSH%%@*}"
    HOSTPORT="${DEV_SSH##*@}"   
    USER="${USERPASS%%:*}"
    PASS="${USERPASS#*:}"

    WORKDIR=/home/${USER}/.flori/build/

    # 检查主机部分是否包含端口
    if [[ "$HOSTPORT" == *:* ]]; then
        HOST="${HOSTPORT%:*}"
        PORT="${HOSTPORT##*:}"
    else
        HOST="$HOSTPORT"
        PORT=""
    fi

}   


function build(){

    # 最后版本
    LATEST=($(git tag))
    INDEX=$((${#LATEST[@]}-1))
    LATEST=${LATEST[INDEX]}
    EXTRA=${LATEST%.*}
    NUM=${LATEST##*.}
    NEW="${EXTRA}.$((NUM+1))"
    echo $LATEST  "->" $NEW
    
 


    # 编译文件
    mkdir -p publish
    git remote get-url build_linux || git remote add build_linux ${USER}@${HOSTPORT}:${WORKDIR} 
    git tag $NEW
    git scp build_linux $NEW

    # git clone https://github.com/harmful-chan/agent --single-branch $VERSION
    # ./sshpass.exe -p ${password} ssh ${username}@${hostport} "mkdir -p $WORKDIR"
    # ./sshpass.exe -p ${password} scp -r $VERSION ${username}@${hostport}:$WORKDIR
    # 
    # pushd $VERSION
    # dotnet publish -r win-x64 -c Release --self-contained true -p:PublishAot=true -p:AssemblyName=agent-cli-${VERSION}-win-x64
    # popd
    # 
    # cp -r $VERSION/Agent.Cli/bin/Release/net8.0/win-x64/publish/* publish
    # rm -rf $VERSION
    # 
    # ./sshpass.exe -p ${password} ssh ${username}@${hostport} "pushd $WORKDIR/$VERSION && dotnet publish -r linux-x64 -c Release --self-contained true -p:PublishAot=true -p:AssemblyName=agent-cli-${VERSION}-linux-x64 && popd"
    # ./sshpass.exe -p ${password} scp -r ${username}@${hostport}:$WORKDIR/$VERSION/Agent.Cli/bin/Release/net8.0/linux-x64/publish/* publish
    # ./sshpass.exe -p ${password} ssh ${username}@${hostport} "rm -rf $WORKDIR/$VERSION"
    # echo 编译完成
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
if [  $# -ge 1 ];then
    if [ "$1" = "build"  ]; then
        setenv $*
        build
    elif [ "$1" = "upload" ]; then
        setenv $*
        upload

    fi
fi






 




