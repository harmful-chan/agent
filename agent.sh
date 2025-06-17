#!/bin/bash
# 服务器监控脚本
# 作者: hans <admin@floribird.com>

# 检查并安装必要工具
function install_dependencies() {
    # 检查是否首次运行
    if [ -f "/tmp/agent_first_run" ]; then
        echo "非首次运行，跳过依赖安装"
        return
    fi
    
    # 检查工具是否已安装
    if command -v curl &> /dev/null && \
       command -v jq &> /dev/null && \
       command -v unzip &> /dev/null && \
       command -v aws &> /dev/null && \
       (command -v lsb_release &> /dev/null || command -v redhat-lsb-core &> /dev/null); then
        echo "所需工具已安装，跳过安装步骤"
        return
    fi

    if command -v apt-get &> /dev/null; then
        # Ubuntu/Debian
        sudo apt-get update
        sudo apt-get install -y curl jq unzip lsb-release
        if ! command -v aws &> /dev/null; then
            curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
            unzip awscliv2.zip
            sudo ./aws/install --bin-dir /usr/local/bin --install-dir /usr/local/aws-cli --update
        fi
    elif command -v yum &> /dev/null; then
        # CentOS/RHEL
        sudo yum install -y curl jq unzip redhat-lsb-core
        if ! command -v aws &> /dev/null; then
            curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
            unzip awscliv2.zip
            sudo ./aws/install --bin-dir /usr/local/bin --install-dir /usr/local/aws-cli --update
        fi
    else
        echo "错误: 不支持的Linux发行版"
        exit 1
    fi
}

# 安装依赖
install_dependencies

# 标记首次运行已完成
touch /tmp/agent_first_run

# 加载环境变量
if [ -f "./.env" ]; then
    source ./.env
else
    echo "错误: 缺少.env配置文件"
    exit 1
fi

# 检查必需的环境变量
required_vars=("FEISHU_APP_ID" "FEISHU_APP_SECRET"  "AWS_ACCESS_KEY_ID" "AWS_SECRET_ACCESS_KEY")
for var in "${required_vars[@]}"; do
    if [ -z "${!var}" ]; then
        echo "错误: 缺少必需的环境变量 $var"
        exit 1
    fi

done

# 获取飞书tenant_access_token
function get_feishu_token() {
    local response=$(curl -s -X POST 'https://open.feishu.cn/open-apis/auth/v3/tenant_access_token/internal' \
        -H 'Content-Type: application/json' \
        -d "{\"app_id\":\"$FEISHU_APP_ID\",\"app_secret\":\"$FEISHU_APP_SECRET\"}")
    
    local token=$(echo $response | jq -r '.tenant_access_token')
    if [ -z "$token" ] || [ "$token" = "null" ]; then
        echo "错误: 获取飞书token失败"
        echo "响应: $response"
        exit 1
    fi
    
    echo $token
}

# 更新飞书表格数据
function update_feishu_table() {
    local token=$1
    local domain=$2
    local status=$3
    
    # 查找记录
    local temp_file=$(mktemp)
    cat > $temp_file <<EOF
{
    "field_names": ["唯一标识/域名"],
    "filter": {
        "conditions": [{
            "field_name": "唯一标识/域名",
            "operator": "is",
            "value": ["$domain"]
        }],
        "conjunction": "and"
    }
}
EOF
    
    local response=$(curl -s -X POST \
        "https://open.feishu.cn/open-apis/bitable/v1/apps/Y51VbNUf3askQ2sDdm9chN8JnAc/tables/tblWxQzjloN1vU06/records/search" \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $token" \
        -d "@$temp_file")
    
    rm -f $temp_file
    
    local record_id=$(echo $response | jq -r '.data.items[0].record_id')
    
    if [ -z "$record_id" ] || [ "$record_id" = "null" ]; then
        # 创建新记录
        local temp_file=$(mktemp)
        cat > $temp_file <<EOF
{
    "fields": {
        "唯一标识/域名": "$domain",
        "状态": "$status",
        "运行时间": "$(uptime -p | sed 's/up //')",
        "架构": "$(uname -m)",
        "内存": "$(free -h | awk '/^Mem:/ {print $2}')",
        "磁盘": "$(df -h / | awk 'NR==2 {print $2}')",
        "区域": "$(curl -s ipinfo.io | jq -r '.country')",
        "系统": "$(lsb_release -d | cut -f2)",
        "CPU": "$(lscpu | grep 'Model name' | cut -d: -f2 | sed 's/^[ \t]*//')",
        "Load": "$(uptime | awk -F'load average: ' '{print $2}')",
        "上传": "$(cat /proc/net/dev | grep -v lo | awk '{sum+=$2} END {print sum/1024/1024 " MB"}')",
        "下载": "$(cat /proc/net/dev | grep -v lo | awk '{sum+=$10} END {print sum/1024/1024 " MB"}')",
        "启动时间": "$(date -d "$(uptime -s)" '+%Y-%m-%d %H:%M:%S')",
        "上报时间": "$(date '+%Y-%m-%d %H:%M:%S')"
    }
}
EOF
        
        curl -s -X POST \
            "https://open.feishu.cn/open-apis/bitable/v1/apps/Y51VbNUf3askQ2sDdm9chN8JnAc/tables/tblWxQzjloN1vU06/records" \
            -H "Authorization: Bearer $token" \
            -H "Content-Type: application/json" \
            -d "@$temp_file"
            
        rm -f $temp_file
    else
        # 更新现有记录
        local temp_file=$(mktemp)
        cat > $temp_file <<EOF
{
    "fields": {
        "状态": "$status",
        "运行时间": "$(uptime -p | sed 's/up //')",
        "Load": "$(uptime | awk -F'load average: ' '{print $2}')",
        "上传": "$(vnstat --oneline | awk -F';' '{print $6}')",
        "下载": "$(vnstat --oneline | awk -F';' '{print $5}')",
        "上报时间": "$(date '+%Y-%m-%d %H:%M:%S')"
    }
}
EOF
        
        curl -s -X PUT \
            "https://open.feishu.cn/open-apis/bitable/v1/apps/Y51VbNUf3askQ2sDdm9chN8JnAc/tables/tblWxQzjloN1vU06/records/$record_id" \
            -H "Authorization: Bearer $token" \
            -H "Content-Type: application/json" \
            -d "@$temp_file"
            
        rm -f $temp_file
    fi
}

# 发送企业微信通知
function send_wechat_notification() {
    local domain=$1
    local status=$2
    
    local content="服务器状态变更通知\n> 域名: <font color=\"comment\">$domain</font>\n> 状态: <font color=\"warning\">$status</font>"
    
    curl -s -X POST "https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=819d9d31-849a-43ca-8d7a-8061fe67d72d" \
        -H 'Content-Type: application/json' \
        -d "{\"msgtype\":\"markdown\",\"markdown\":{\"content\":\"$content\"}}"
}

# 更新AWS Route53记录
function update_aws_record() {
    local domain=$1
    local ip=$2
    local action=$3  # CREATE或UPSERT
    local hosted_zone_id=$4
    
    # 创建临时JSON文件
    local temp_file=$(mktemp)
    cat > $temp_file <<EOF
{
    "Comment": "自动更新DNS记录",
    "Changes": [
        {
            "Action": "$action",
            "ResourceRecordSet": {
                "Name": "$domain",
                "Type": "A",
                "TTL": 60,
                "ResourceRecords": [
                    {
                        "Value": "$ip"
                    }
                ]
            }
        }
    ]
}
EOF
    
    # 执行AWS CLI命令
    aws route53 change-resource-record-sets \
        --hosted-zone-id $hosted_zone_id \
        --change-batch file://$temp_file
    
    # 清理临时文件
    rm -f $temp_file
}

# 主监控函数
function monitor_servers() {
    # 获取飞书token
    local token=$(get_feishu_token)
    
    # 从环境变量获取监控域名
    local domain="$DEV_DOMAIN"
    if [ -z "$domain" ]; then
        echo "错误: 未设置DEV_DOMAIN环境变量"
        exit 1
    fi
    
    # 获取服务器状态 (示例逻辑，实际应根据需求实现)
    local status="在线"
    local ip="$(curl -s ifconfig.me)"
    
    # 更新飞书表格
    update_feishu_table "$token" "$domain" "$status"
    
    # 发送微信通知
    send_wechat_notification "$domain" "$status"
    
    # 如果是上线状态，更新DNS记录
    if [ "$status" = "在线" ]; then
        local hosted_zone_id=$(aws route53 list-hosted-zones | jq -r '.HostedZones[0].Id')
        update_aws_record "$domain" "$ip" "UPSERT" "$hosted_zone_id"
    fi
}

monitor_servers