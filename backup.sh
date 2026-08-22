#!/usr/bin/env bash

# ==============================================================================
# MEDAI — AUTOMATED POSTGRESQL DATABASE BACKUP & ROTATION SCRIPT
# Database: medaidb
# Backup Dir: /var/backups/medai
# Retention: 30 Days
# ==============================================================================

set -e

BACKUP_DIR="/var/backups/medai"
TIMESTAMP=$(date +"%Y-%m-%d_%H%M%S")
BACKUP_FILE="${BACKUP_DIR}/medaidb_backup_${TIMESTAMP}.sql.gz"
CONTAINER_NAME="medai-postgres"
DB_USER="postgres"
DB_NAME="medaidb"
RETENTION_DAYS=30

echo "📦 Starting MedAI PostgreSQL Backup at $(date)..."

# Ensure backup directory exists
mkdir -p "${BACKUP_DIR}"

# Execute pg_dump inside postgres container and compress
docker exec "${CONTAINER_NAME}" pg_dump -U "${DB_USER}" "${DB_NAME}" | gzip > "${BACKUP_FILE}"

echo "✅ Backup created successfully: ${BACKUP_FILE}"
echo "📊 Backup file size: $(du -h "${BACKUP_FILE}" | cut -f1)"

# Rotate old backups older than 30 days
echo "🧹 Removing backups older than ${RETENTION_DAYS} days..."
find "${BACKUP_DIR}" -name "medaidb_backup_*.sql.gz" -mtime +${RETENTION_DAYS} -exec rm -f {} \;

echo "🎉 Database backup process completed successfully at $(date)."
