# MEDAI — Full-Stack AI Healthcare Startup Platform

> **"An intelligent healthcare ecosystem that connects patients, doctors, clinics and medical information through AI."**

MEDAI is a production-grade HealthTech platform engineered with **Clean Architecture, C# .NET 10, Entity Framework Core 10, PostgreSQL**, and **Next.js 15+ (TypeScript, Tailwind CSS, shadcn/ui)**.

---

## 🚀 Key Platform Features

- **Clean Architecture Solution (`MedAI.sln`)**:
  - `MedAI.Domain`: Core Domain Entities, Value Objects, Enums (`User`, `PatientProfile`, `DoctorProfile`, `Clinic`, `Appointment`, `MedicalRecord`, `MedicalDocument`, `LabResult`, `Medication`, `Prescription`, `HealthEvent`, `FamilyMember`, `Notification`, `AISession`, `AIMessage`, `DoctorNote`, `MedicalArticle`, `AuditLog`).
  - `MedAI.Application`: Business Services, FluentValidation Request DTOs, `IAIService` Abstraction, Standardized API Responses (`ApiResponse<T>`, `PagedResponse<T>`).
  - `MedAI.Infrastructure`: EF Core DbContext with PostgreSQL/InMemory provider, BCrypt Password Hashing, JWT Token Generator, Audit Logging, DbInitializer with rich DEMO data.
  - `MedAI.API`: Controllers, Serilog, Exception Handling Middleware, JWT Authorization & Swagger UI with Bearer Authentication button.
  - `MedAI.Tests`: xUnit test suite for Auth, RBAC, Appointment Scheduling, and AI Safety disclaimers.

- **Intelligent AI Clinical Assistant (`IAIService`)**:
  - **Session-Aware AI Health Chat**: Answers health queries with emergency detection & clinical disclaimers.
  - **Symptom Analyzer & Triage**: Evaluates symptoms, risk levels (Low, Moderate, High, Emergency), and recommended next steps without issuing final medical diagnoses.
  - **Lab Result Explainer**: Breaks down complex lab values against reference ranges and generates doctor questions.
  - **Medical Document AI Summarizer**: Extracts key clinical findings from uploaded records.
  - **Doctor Clinical Copilot Brief**: Generates pre-consultation patient summaries for physicians.

- **Next.js 15+ Frontend**:
  - Direct PostgreSQL access is strictly forbidden; all interactions flow cleanly through ASP.NET Core Web API (`NEXT_PUBLIC_API_URL`).
  - Dedicated service layer (`services/api.ts`, `services/allServices.ts`) with automatic JWT injection.
  - Medical Design System (`#2563EB` primary, `#0F766E` secondary, `#06B6D4` accent) with glassmorphism cards and micro-animations.

---

## 🔑 Demo Accounts

The database comes pre-seeded with realistic demonstration accounts:

| Role | Email | Password | Features Accessible |
| :--- | :--- | :--- | :--- |
| **Patient** | `patient@medai.com` | `Patient123!` | Health Passport, AI Assistant, Symptom Triage, Appointments, Lab Explainer |
| **Doctor** | `doctor@medai.com` | `Doctor123!` | Doctor Hub, Patient Roster, AI Copilot Brief, Clinical Notes |
| **SuperAdmin** | `admin@medai.com` | `Admin123!` | System Analytics, User Directory, Verification, Security Audit Logs |

---

## 🛠️ Quick Start Instructions

### 1. Run Backend API (.NET 10)
```bash
cd c:/Users/user/Downloads/MedAi
dotnet restore
dotnet build
dotnet run --project src/MedAI.API/MedAI.API.csproj
```
The API will start at `https://localhost:7001`. Open Swagger UI at:
👉 **`https://localhost:7001/swagger`**

### 2. Run Backend Unit Tests
```bash
dotnet test
```

### 3. Run Next.js Frontend
```bash
cd frontend
npm install
npm run dev
```
Open **`http://localhost:3000`** in your browser.

---

## 🐳 Docker Deployment

To launch the complete infrastructure (API, Next.js Frontend, PostgreSQL, Redis) using Docker Compose:

```bash
docker-compose up --build -d
```

---

## 🛡️ Medical AI Safety & Privacy Policy

1. **AI is an Assistant, Not a Doctor**: AI output is strictly for educational clarification and clinical workflow optimization. AI never issues independent medical diagnoses or prescribes medication.
2. **Role-Based Security (RBAC)**: Patients access only their data; Doctors access assigned patients; Admins access platform metadata.
3. **Audit Logging**: Sensitive medical operations are automatically logged in `AuditLogs`.
