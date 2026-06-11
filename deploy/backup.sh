#!/bin/bash
# ==========================================
# 数据库备份脚本
# 在 docker-compose 宿主机上运行
# 使用: bash deploy/backup.sh
# 可配合 cron 定时执行
# ==========================================

BACKUP_DIR="./deploy/backup"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
DB_NAME="mblog"
DB_USER="mblog_user"
CONTAINER="mblog-postgres"

mkdir -p "$BACKUP_DIR"

echo "开始备份数据库: $DB_NAME"

# 使用 pg_dump 在容器中执行备份
docker exec "$CONTAINER" pg_dump -U "$DB_USER" "$DB_NAME" > "$BACKUP_DIR/${DB_NAME}_${TIMESTAMP}.sql"

if [ $? -eq 0 ]; then
    echo "备份成功: $BACKUP_DIR/${DB_NAME}_${TIMESTAMP}.sql"
    # 压缩备份
    gzip "$BACKUP_DIR/${DB_NAME}_${TIMESTAMP}.sql"
    echo "压缩完成: $BACKUP_DIR/${DB_NAME}_${TIMESTAMP}.sql.gz"
    # 保留最近 30 天的备份，删除更早的
    find "$BACKUP_DIR" -name "${DB_NAME}_*.sql.gz" -mtime +30 -delete
    echo "已清理 30 天前的备份"
else
    echo "备份失败!"
    exit 1
fi