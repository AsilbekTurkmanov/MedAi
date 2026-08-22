#!/usr/bin/env bash

# ==============================================================================
# MEDAI — CONTABO VPS ONE-CLICK DEPLOYMENT SCRIPT
# Repository: https://github.com/AsilbekTurkmanov/MedAi.git
# ==============================================================================

set -e

echo "🚀 Starting MEDAI Platform Deployment on VPS..."

# 1. Pull latest code from GitHub
echo "📥 Pulling latest code from GitHub..."
git pull origin main

# 2. Build and restart Docker containers
echo "🐳 Building Docker containers..."
docker compose down --remove-orphans
docker compose build --no-cache
docker compose up -d

# 3. Setup Automated Cron Backup (Daily at 3:00 AM)
echo "⏰ Setting up automated daily Database Backup Cron Job..."
chmod +x backup.sh
(crontab -l 2>/dev/null | grep -v "backup.sh"; echo "0 3 * * * $(pwd)/backup.sh >> /var/log/medai_backup.log 2>&1") | crontab -

# 4. Display status
echo "✅ Deployment & Automated Backup Setup Successful!"
docker compose ps
