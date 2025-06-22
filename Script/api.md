### Cli53 命令行工具
```shell 列出所有域名信息

```



### AWS Route53 API
```shell aws cli 安装步骤
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
sudo ./aws/install --bin-dir /usr/local/bin --install-dir /usr/local/aws-cli --update
```
```shell 列出所有域名信息
aws route53  list-hosted-zones
// 响应JSON
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
```
```shell 列出域名记录
aws route53 list-resource-record-sets --hosted-zone-id /hostedzone/Z0780784V5E3O046QSG6
// 响应JSON
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
```
```shell 创建A记录
// create-a.json 文件内容
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

aws route53 change-resource-record-sets --hosted-zone-id /hostedzone/Z0780784V5E3O046QSG6 --change-batch file://create-a.json
// 请求响应JSON
{
    "ChangeInfo": {
        "Id": "/change/C08756482QRBCSKZ6BZAZ",
        "Status": "PENDING",
        "SubmittedAt": "2025-06-17T08:58:20.826000+00:00",
        "Comment": "Testing public hosted zone in BJS"
    }
}
```
```shell 更新A记录
// upsert-a.json 文件内容
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
// 请求命令
aws route53 change-resource-record-sets --hosted-zone-id /hostedzone/Z0780784V5E3O046QSG6 --change-batch file://upsert-a.json
// 请求响应JSON
{
    "ChangeInfo": {
        "Id": "/change/C00798981MSN51FIXHTO",
        "Status": "PENDING",
        "SubmittedAt": "2025-06-17T09:00:54.893000+00:00",
        "Comment": "Testing public hosted zone in BJS"
    }
}
```

### 企业微信推送信息API
```shell
// 819d9d31-849a-43ca-8d7a-8061fe67d72d 是固定的;
POST https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=819d9d31-849a-43ca-8d7a-8061fe67d72d
Content-Type: application/json
{
    "msgtype": "markdown",
    "markdown": {
        "content": "实时新增用户反馈<font color=\"warning\">132例</font>，请相关同事注意。\n
        > 类型:<font color=\"comment\">用户反馈</font>\n
        > 普通用户反馈:<font color=\"comment\">117例</font>\n
        > VIP用户反馈:<font color=\"comment\">15例</font>"
    }
}
```

### 飞书多维表格API
```shell 获取 tenant_access_token; 最大有效期是 2 小时;
GET https://open.feishu.cn/open-apis/auth/v3/tenant_access_token/internal
Content-Type: application/json
{
    "app_id": $FEISHU_APP_ID,
    "app_secret": $FEISHU_APP_SECRET
}
// 成功响应JSON
{
  "code": 0,
  "expire": 7047,
  "msg": "ok",
  "tenant_access_token": "t-g1046hh7CMLE5GY2WPMXGUXCVTR6F6BTLQC35WRU"
}
// 失败响应JSON
{
  "code": 10014,
  "msg": "app secret invalid"
}
```

```shell 获取多维表格ID(obj_token)
// UuMcwn9jhi8PQVkd7LRc5JxVnyh 是固定的;
GET https://open.feishu.cn/open-apis/wiki/v2/spaces/get_node?obj_type=wiki&token=UuMcwn9jhi8PQVkd7LRc5JxVnyh
Authorization: Bearer t-g1046hh7CMLE5GY2WPMXGUXCVTR6F6BTLQC35WRU
// 响应json
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
```

```shell 查找记录 
// Y51VbNUf3askQ2sDdm9chN8JnAc 是固定的;
// tblWxQzjloN1vU06 是固定的;
POST https://open.feishu.cn/open-apis/bitable/v1/apps/Y51VbNUf3askQ2sDdm9chN8JnAc/tables/tblWxQzjloN1vU06/records/search?page_size=20
Content-Type: application/json
Authorization: Bearer t-g1046hh7CMLE5GY2WPMXGUXCVTR6F6BTLQC35WRU
{
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
}
// 查找成功 响应json
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
// 查找失败 响应JSON
{
  "code": 0,
  "data": {
    "has_more": false,
    "items": [],
    "total": 0
  },
  "msg": "success"
}
```

```shell 更新记录
// Y51VbNUf3askQ2sDdm9chN8JnAc 是固定的;
// tblWxQzjloN1vU06 是固定的;
PUT https://open.feishu.cn/open-apis/bitable/v1/apps/Y51VbNUf3askQ2sDdm9chN8JnAc/tables/tblWxQzjloN1vU06/records/recOIGC4Uv
Content-Type: application/json
Authorization: Bearer t-g1046hh7CMLE5GY2WPMXGUXCVTR6F6BTLQC35WRU
{
    "fields": {
        "唯一标识/域名": "s3.hans.fc.fb.dev.153246.com",
        "备注": "更新被欸住"
    }
}
// 更新成功响应JSON
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
```

```shell 创建记录
// Y51VbNUf3askQ2sDdm9chN8JnAc 是固定的;
// tblWxQzjloN1vU06 是固定的;
POST https://open.feishu.cn/open-apis/bitable/v1/apps/Y51VbNUf3askQ2sDdm9chN8JnAc/tables/tblWxQzjloN1vU06/records
Authorization: Bearer t-g1046hh7CMLE5GY2WPMXGUXCVTR6F6BTLQC35WRU
Content-Type: application/json
{
    "fields": {
        "唯一标识/域名": "s3.hans.fc.fb.dev.153246.com"
    }
}
// 创建成功响应JSON
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
```

