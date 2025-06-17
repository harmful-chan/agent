## 任务/目的/需求
你是公司的运维开发专家，擅长书写标准的linux shell 脚本；
你的名字是`hans`，工作邮箱`admin@floribird.com`；
现在需要一个脚本，使用飞书多维表格API保存服务器数据，当服务器上线，离线时通过企业微信群API通知；
服务器上线时同时通过 AWS域名管理API 给对应的域名添加 A 记录；在脚本中需要设置定时任务，每1分钟执行一次；
AWS 域名管理支持是同http请求操作数据，使用方法参考 ${AWS Route53 API}
企业微信支持webhook推送通知，使用方法参考 ${企业微信推送信息API};
飞书多维表格是支持http请求操作表格，使用方法参考 ${飞书多维表格API};
飞书多维表格字段参考 ${飞书表格字段};
### 使用方法
安装 `curl -L https://raw.githubusercontent.com/harmful-chan/agent/agent.sh -o agent.sh && chmod +x agent.sh`
使用：`DEV_DOMAIN=s1.hans.fc.fb.dev.153246.com && bash agent.sh`
### 步骤
1. 加载环境变量
2. 获取服务器信息
3. 更新飞书多为表格
4. 更新域名记录

## 公司规范
项目使用github做版本管理，使用gitflow工作流;
`./.env` 文件保存所需的隐私信息，代码内需要加载环境变量，但不能影响系统全局;
`./agent.sh` 是主要执行脚本，非别要情况不能创建其他文件；
`./requirements.md` 是项目需求文档；
写代码时使用中文做代码注释；http请求json格式的body先保存到临时文件；在发送请求
执行任意脚本错误时候直接退出，不继续执行；输出适当的提示信息；

## AWS Route53 API
#### 列出所有域名信息
##### 请求命令
aws route53  list-hosted-zones
##### 响应JSON
{
    "HostedZones": [
        {
            "Id": "/hostedzone/Z0780784V5E3O046QSG6",
            "Name": "153246.com.",
            "CallerReference": "12def7f6-4db6-4e5b-8e3c-64d1b8394c3e",
            "Config": {
                "Comment": "",
                "PrivateZone": false
            },
            "ResourceRecordSetCount": 2
        }
    ]
}
#### 列出域名记录
##### 请求命令
aws route53 list-resource-record-sets --hosted-zone-id /hostedzone/Z0780784V5E3O046QSG6
##### 响应JSON
{
    "ResourceRecordSets": [
        {
            "Name": "153246.com.",
            "Type": "NS",
            "TTL": 172800,
            "ResourceRecords": [
                {
                    "Value": "ns-324.awsdns-40.com."
                },
                {
                    "Value": "ns-1119.awsdns-11.org."
                },
                {
                    "Value": "ns-921.awsdns-51.net."
                },
                {
                    "Value": "ns-1811.awsdns-34.co.uk."
                }
            ]
        },
        {
            "Name": "153246.com.",
            "Type": "SOA",
            "TTL": 900,
            "ResourceRecords": [
                {
                    "Value": "ns-324.awsdns-40.com. awsdns-hostmaster.amazon.com. 1 7200 900 1209600 86400"
                }
            ]
        },
        {
            "Name": "s3.hans.fc.fb.dev.153246.com.",
            "Type": "A",
            "TTL": 60,
            "ResourceRecords": [
                {
                    "Value": "192.168.2.2"
                }
            ]
        }
    ]
}
#### 创建A记录
##### ./create-a.json 文件内容
{
    "Comment": "Testing public hosted zone in BJS", 
    "Changes": [
        {
            "Action": "CREATE", 
            "ResourceRecordSet": 
                {
                    "Name": "s3.hans.fc.fb.dev.153246.com", 
                    "Type": "A",
                    "TTL": 60,
                    "ResourceRecords": [
                        {
                            "Value": "10.200.0.1"
                            } 
                        ]
                } 
        }
    ] 
}
##### 请求命令
aws route53 change-resource-record-sets --hosted-zone-id /hostedzone/Z0780784V5E3O046QSG6 --change-batch file://create-a.json
##### 请求响应JSON
{
    "ChangeInfo": {
        "Id": "/change/C08756482QRBCSKZ6BZAZ",
        "Status": "PENDING",
        "SubmittedAt": "2025-06-17T08:58:20.826000+00:00",
        "Comment": "Testing public hosted zone in BJS"
    }
}
#### 创建A记录
##### ./upsert-a.json 文件内容
{
    "Comment": "Testing public hosted zone in BJS", 
    "Changes": [
        {
            "Action": "UPSERT", 
            "ResourceRecordSet": 
                {
                    "Name": "s3.hans.fc.fb.dev.153246.com", 
                    "Type": "A",
                    "TTL": 60,
                    "ResourceRecords": [
                        {
                            "Value": "192.168.0.100"
                            } 
                        ]
                } 
        }
    ] 
}
##### 请求命令
aws route53 change-resource-record-sets --hosted-zone-id /hostedzone/Z0780784V5E3O046QSG6 --change-batch file://upsert-a.json
##### 请求响应JSON
{
    "ChangeInfo": {
        "Id": "/change/C00798981MSN51FIXHTO",
        "Status": "PENDING",
        "SubmittedAt": "2025-06-17T09:00:54.893000+00:00",
        "Comment": "Testing public hosted zone in BJS"
    }
}
飞书表格字段

## 飞书表格字段
### 说明
冒号'：'前面的是字段名称，后面的是服务器信息，需要实时获取;
<字段></字段>内的数据都是必须的；
<字段>
唯一标识/域名：s1.hans.fc.fb.dev.153246.com
状态：在线
运行时间：14 小时
架构：x86_64
内存：7.56 GiB
磁盘：117.99 GiB
区域：CN
系统：ubuntu
CPU：Intel(R) Xeon(R) Platinum 8255C CPU @ 2.50GHz 4 Virtual Core
Load：0.12 / 0.09 / 0.03
上传：39.64 MiB
下载：54.35 MiB
启动时间：2025-06-17 00:08:30
上报时间：2025-06-17 14:32:37
企业微信推送信息API
</字段>

## 企业微信推送信息API
#### 推送消息
##### 请求命令
curl 'https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=819d9d31-849a-43ca-8d7a-8061fe67d72d' \
-H 'Content-Type: application/json' \
-d '
{
    "msgtype": "markdown",
    "markdown": {
        "content": "实时新增用户反馈<font color=\"warning\">132例</font>，请相关同事注意。\n
        > 类型:<font color=\"comment\">用户反馈</font>\n
        > 普通用户反馈:<font color=\"comment\">117例</font>\n
        > VIP用户反馈:<font color=\"comment\">15例</font>"
    }
}'

## 飞书多维表格API
#### 获取 tenant_access_token 最大有效期是 2 小时。
##### 请求命令
curl -i -X POST 'https://open.feishu.cn/open-apis/auth/v3/tenant_access_token/internal' \
-H 'Content-Type: application/json' \
-d '{
        "app_id": $FEISHU_APP_ID,
        "app_secret": $FEISHU_APP_SECRET
}'
##### 成功响应JSON
{
  "code": 0,
  "expire": 7047,
  "msg": "ok",
  "tenant_access_token": "t-g1046hh7CMLE5GY2WPMXGUXCVTR6F6BTLQC35WRU"
}
##### 失败响应JSON
{
  "code": 10014,
  "msg": "app secret invalid"
}
##### 请求命令
curl -i -X GET 'https://open.feishu.cn/open-apis/wiki/v2/spaces/get_node?obj_type=wiki&token=UuMcwn9jhi8PQVkd7LRc5JxVnyh' \
-H 'Authorization: Bearer t-g1046hh7CMLE5GY2WPMXGUXCVTR6F6BTLQC35WRU'
##### 响应json
{
  "code": 0,
  "data": {
    "node": {
      "creator": "ou_c514778b49d1620791110276dabb56a3",
      "has_child": false,
      "node_create_time": "1750137529",
      "node_creator": "ou_c514778b49d1620791110276dabb56a3",
      "node_token": "UuMcwn9jhi8PQVkd7LRc5JxVnyh",
      "node_type": "origin",
      "obj_create_time": "1750137529",
      "obj_edit_time": "1750141882",
      "obj_token": "Y51VbNUf3askQ2sDdm9chN8JnAc",
      "obj_type": "bitable",
      "origin_node_token": "UuMcwn9jhi8PQVkd7LRc5JxVnyh",
      "origin_space_id": "7439684157899636739",
      "owner": "ou_c514778b49d1620791110276dabb56a3",
      "parent_node_token": "GwwawVpleiLcnJkLOwicsiT3nQb",
      "space_id": "7439684157899636739",
      "title": "服务器列表"
    }
  },
  "msg": "success"
}
#### 查找记录
##### 请求
curl -i -X POST 'https://open.feishu.cn/open-apis/bitable/v1/apps/Y51VbNUf3askQ2sDdm9chN8JnAc/tables/tblWxQzjloN1vU06/records/search?page_size=20' \
-H 'Content-Type: application/json' \
-H 'Authorization: Bearer t-g1046hh7CMLE5GY2WPMXGUXCVTR6F6BTLQC35WRU' \
-d '{
        "automatic_fields": false,
        "field_names": [
                "唯一标识/域名"
        ],
        "filter": {
                "conditions": [
                        {
                                "field_name": "唯一标识/域名",
                                "operator": "is",
                                "value": [
                                        "s1.hans.fc.fb.dev.153246.com"
                                ]
                        }
                ],
                "conjunction": "and"
        },
        "sort": [
                {
                        "desc": true,
                        "field_name": "唯一标识/域名"
                }
        ],
        "view_id": "vewPimz5Tb"
}'
##### 查找成功 响应json
{
  "code": 0,
  "data": {
    "has_more": false,
    "items": [
      {
        "fields": {
          "唯一标识/域名": [
            {
              "link": "http://s1.hans.fc.fb.dev.153246.com",
              "text": "s1.hans.fc.fb.dev.153246.com",
              "type": "url"
            }
          ]
        },
        "record_id": "recOIGC4Uv"
      }
    ],
    "total": 1
  },
  "msg": "success"
}
##### 查找失败 响应JSON
{
  "code": 0,
  "data": {
    "has_more": false,
    "items": [],
    "total": 0
  },
  "msg": "success"
}
#### 更新记录
##### 请求
curl -i -X PUT 'https://open.feishu.cn/open-apis/bitable/v1/apps/Y51VbNUf3askQ2sDdm9chN8JnAc/tables/tblWxQzjloN1vU06/records/recOIGC4Uv' \
-H 'Content-Type: application/json' \
-H 'Authorization: Bearer t-g1046hh7CMLE5GY2WPMXGUXCVTR6F6BTLQC35WRU' \
-d '{
        "fields": {
                "唯一标识/域名": "s3.hans.fc.fb.dev.153246.com",
                "备注": "更新被欸住"
        }
}'
##### 更新成功响应JSON
{
  "code": 0,
  "data": {
    "record": {
      "fields": {
        "唯一标识/域名": "s3.hans.fc.fb.dev.153246.com",
        "备注": "这是一个备注"
      },
      "id": "recuOmqMuH0dRJ",
      "record_id": "recuOmqMuH0dRJ"
    }
  },
  "msg": "success"
}
#### 创建记录
##### 请求
curl -i -X POST 'https://open.feishu.cn/open-apis/bitable/v1/apps/Y51VbNUf3askQ2sDdm9chN8JnAc/tables/tblWxQzjloN1vU06/records' \
-H 'Authorization: Bearer t-g1046hh7CMLE5GY2WPMXGUXCVTR6F6BTLQC35WRU' \
-H 'Content-Type: application/json' \
-d '{
        "fields": {
                "唯一标识/域名": "s3.hans.fc.fb.dev.153246.com"
        }
}'
##### 创建成功响应JSON
{
  "code": 0,
  "data": {
    "record": {
      "fields": {
        "唯一标识/域名": "s3.hans.fc.fb.dev.153246.com"
      },
      "id": "recuOmqMuH0dRJ",
      "record_id": "recuOmqMuH0dRJ"
    }
  },
  "msg": "success"
}

