# 🩺 MEDAI — Full-Stack AI Healthcare Startup Platform

> **Patient + Doctor + Clinic + AI Ekotizimini Birlashtiruvchi Professional HealthTech Platforma**

![MedAI Architecture](https://img.shields.io/badge/.NET_10-ASP.NET_Core_API-512BD4?logo=dotnet)
![Next.js 15](https://img.shields.io/badge/Next.js_15-React_19-000000?logo=next.js)
![Docker](https://img.shields.io/badge/Docker_Compose-Containerized-2496ED?logo=docker)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)

---

## 🚀 GitHub Repository-ga Joylash (Push Instructions)

Loyiha allaqachon git omboriga tayyorlandi va commit qilindi. GitHub-ga push qilish uchun terminalda quyidagi buyruqni bajaring:

```bash
git push -u origin main --force
```

---

## 🌐 Contabo VPS Serverda Deploy Qilish (One-Click Docker Deployment)

Contabo VPS serveringizda SSH orqali quyidagi 4 ta tezkor qadamni bajaring:

### 1. VPS Serverga SSH orqali ulaning:
```bash
ssh root@YOUR_CONTABO_SERVER_IP
```

### 2. Loyihani GitHub-dan Clone qiling:
```bash
git clone https://github.com/AsilbekTurkmanov/MedAi.git
cd MedAi
```

### 3. Deploy Script-ni ishga tushiring (yoki Docker Compose):
```bash
chmod +x deploy.sh
./deploy.sh
```

*Yoki qo'lda Docker Compose orqali:*
```bash
docker compose up -d --build
```

---

## 🔑 Demo Kirish Ma'lumotlari (Pre-seeded Demo Accounts)

| Rol | Email | Parol | Ruxsat va Imkoniyatlar |
| :--- | :--- | :--- | :--- |
| **Bemor (Patient)** | `patient@medai.com` | `Patient123!` | Health Passport, AI Assistant, Symptom Triage, Appointments |
| **Shifokor (Doctor)** | `doctor@medai.com` | `Doctor123!` | Doctor Hub, Patient Roster, AI Copilot Brief, Prescriptions |
| **Admin (SuperAdmin)** | `admin@medai.com` | `Admin123!` | Analytics, User Directory, Clinic Management, Security Audit Logs |

---

## 🛠️ Loyiha Texnologik Steki

### Backend (.NET 10 Clean Architecture)
- **C# .NET 10** ASP.NET Core Web API
- **Entity Framework Core 10** (Npgsql PostgreSQL + InMemory Fallback)
- **JWT Bearer Authentication** + Refresh Token Rotation
- **Serilog Logging** & Centralized Middleware Exception Handling
- **Swagger / OpenAPI** Swagger UI Documentation

### Frontend (Next.js 15+ App Router)
- **Next.js 15.1** (TypeScript, Tailwind CSS)
- **i18n Multilingual**: 🇺🇿 O'zbekcha (Default), 🇷🇺 Русский, 🇬🇧 English
- **Theme Provider**: ☀️ Day (Light) & 🌙 Night (Dark) Modes
- **Lucide Icons** & Glassmorphism Responsive UI
