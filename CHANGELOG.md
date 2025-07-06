# 更新日志

## [1.0.0] - 很久以前

### 新增

- 

## [1.1.0] - 2025-07-05

### 增加
- `.env` 增加 VERSION
- `.env` 增加 DEV_SSH

### 修改

- 以表格ID作为唯一标识
- 不自动更新或创建域名A记录
- publsh.sh 更名 dev.sh
- 构建: `bash dev.sh build` 自动打标签，构建，版本-aplha.序号
- 发布: `bash dev.sh publish` 获取最新标签，发布