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

# 3. Display status
echo "✅ Deployment Successful!"
docker compose ps
