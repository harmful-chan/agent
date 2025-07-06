#!/bin/bash

# v1.0.0-alpha.1 hans:123456@192.168.2.248
set -e


function setenv() {
    source $(dirname $(readlink -f $0))/.env 
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


    # 可执行文件目录
    DIR=$(dirname $(readlink -f $0))
    # 最后版本
    LATEST=($(git tag | grep ${VERSION} | sort -V))
    INDEX=$((${#LATEST[@]}-1))
    LATEST=${LATEST[INDEX]}
    EXTRA=${LATEST%.*}
    NUM=$(echo "$LATEST" | grep -oE '[0-9]+$')
    NEW="${EXTRA}.$((NUM+1))"
    echo $LATEST  "->" $NEW
    
 


    # 编译文件
    mkdir -p build
    git tag $NEW
    git archive --format=tar.gz --prefix=${NEW}/ --output=build/${NEW}.tar.gz ${NEW}

    # 拷贝代码
    # git clone https://github.com/harmful-chan/agent --single-branch $VERSION
    $DIR/sshpass.exe -p ${PASS} ssh ${USER}@${HOSTPORT} "mkdir -p $WORKDIR"
    $DIR/sshpass.exe -p ${PASS} scp -r build/${NEW}.tar.gz ${USER}@${HOSTPORT}:$WORKDIR
    # 

    # 解压并编译代码 windows
    pushd build
    tar -xvf ${NEW}.tar.gz
    pushd ${NEW}
    dotnet publish -r win-x64 -c Release --self-contained true -p:PublishAot=true -p:AssemblyName=agent-cli-${NEW}-win-x64
    popd
     
    cp -r $NEW/src/Agent.Cli/bin/Release/net8.0/win-x64/publish/* .
    rm -rf $NEW
     
    $DIR/sshpass.exe -p ${PASS} ssh ${USER}@${HOSTPORT} "pushd $WORKDIR && tar -xvf ${NEW}.tar.gz &&  pushd $NEW && dotnet publish -r linux-x64 -c Release --self-contained true -p:PublishAot=true -p:AssemblyName=agent-cli-${NEW}-linux-x64 && popd"
    $DIR/sshpass.exe -p ${PASS} scp -r ${USER}@${HOSTPORT}:$WORKDIR/$NEW/src/Agent.Cli/bin/Release/net8.0/linux-x64/publish/* .
    $DIR/sshpass.exe -p ${PASS} ssh ${USER}@${HOSTPORT} "rm -rf $WORKDIR/${NEW}*"
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






 




