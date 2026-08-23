using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedAI.Application.Interfaces;
using MedAI.Domain.Entities;
using MedAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedAI.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(MedAiDbContext context, IPasswordHasher hasher)
    {
        if (await context.Users.AnyAsync(u => u.Email == "patient@medai.com")) return;

        // 1. Seed Clinic
        var clinic = new Clinic
        {
            Id = Guid.NewGuid(),
            Name = "MedAI Central Specialty Clinic (DEMO)",
            Description = "State-of-the-art medical clinic equipped with AI diagnostic support.",
            Address = "100 Innovation Way, Healthcare District, Suite 500",
            Phone = "+1 (800) 555-MEDAI",
            Email = "contact@clinic.medai.io",
            Website = "https://medai.health",
            CreatedAt = DateTime.UtcNow
        };
        context.Clinics.Add(clinic);

        // 2. Seed SuperAdmin User
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@medai.com",
            PasswordHash = hasher.HashPassword("Admin123!"),
            FirstName = "Super",
            LastName = "Admin",
            PhoneNumber = "+1 555-0100",
            DateOfBirth = new DateTime(1985, 5, 20),
            Gender = "Other",
            Role = UserRole.SuperAdmin,
            PreferredLanguage = "en",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(adminUser);

        // 3. Seed Doctor User & Profile
        var doctorUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "doctor@medai.com",
            PasswordHash = hasher.HashPassword("Doctor123!"),
            FirstName = "Dr. Alexander",
            LastName = "Vance",
            PhoneNumber = "+1 555-0101",
            DateOfBirth = new DateTime(1980, 3, 15),
            Gender = "Male",
            Role = UserRole.Doctor,
            PreferredLanguage = "en",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(doctorUser);

        var doctorProfile = new DoctorProfile
        {
            Id = Guid.NewGuid(),
            UserId = doctorUser.Id,
            Specialization = "Cardiology & Preventive Medicine",
            LicenseNumber = "MD-CARDIO-88942-DEMO",
            ExperienceYears = 14,
            Bio = "Senior Consultant Cardiologist specializing in preventive heart health and AI-guided diagnostics.",
            ClinicId = clinic.Id,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };
        context.DoctorProfiles.Add(doctorProfile);

        // 4. Seed Patient User & Profile
        var patientUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "patient@medai.com",
            PasswordHash = hasher.HashPassword("Patient123!"),
            FirstName = "Sarah",
            LastName = "Jenkins",
            PhoneNumber = "+1 555-0102",
            DateOfBirth = new DateTime(1992, 8, 12),
            Gender = "Female",
            Role = UserRole.Patient,
            PreferredLanguage = "en",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(patientUser);

        var patientProfile = new PatientProfile
        {
            Id = Guid.NewGuid(),
            UserId = patientUser.Id,
            BloodType = "A+",
            EmergencyContactName = "Robert Jenkins",
            EmergencyContactPhone = "+1 555-0199",
            Address = "742 Evergreen Terrace, Medical Heights",
            CreatedAt = DateTime.UtcNow
        };
        context.PatientProfiles.Add(patientProfile);

        // 5. Seed Appointments
        var appt1 = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patientProfile.Id,
            DoctorId = doctorProfile.Id,
            ClinicId = clinic.Id,
            AppointmentDate = DateTime.UtcNow.AddDays(2).Date,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(10, 30, 0),
            Status = AppointmentStatus.Confirmed,
            Reason = "Routine Cardiovascular Follow-Up (DEMO)",
            Notes = "Patient requesting review of recent cholesterol numbers and BP monitoring.",
            CreatedAt = DateTime.UtcNow
        };

        var appt2 = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patientProfile.Id,
            DoctorId = doctorProfile.Id,
            ClinicId = clinic.Id,
            AppointmentDate = DateTime.UtcNow.AddDays(-14).Date,
            StartTime = new TimeSpan(14, 0, 0),
            EndTime = new TimeSpan(14, 30, 0),
            Status = AppointmentStatus.Completed,
            Reason = "Initial AI Symptom Assessment Review (DEMO)",
            Notes = "Completed intake review. Ordered lipid panel and blood glucose.",
            CreatedAt = DateTime.UtcNow.AddDays(-14)
        };
        context.Appointments.AddRange(appt1, appt2);

        // 6. Seed Medical Record
        var record = new MedicalRecord
        {
            Id = Guid.NewGuid(),
            PatientId = patientProfile.Id,
            DoctorId = doctorProfile.Id,
            AppointmentId = appt2.Id,
            Title = "Annual Cardiovascular Evaluation (DEMO)",
            Description = "Comprehensive physical exam and diagnostic panel review.",
            DiagnosisNotes = "Mild essential hypertension. Normal EKG rhythm. Patient advised to maintain low-sodium diet.",
            CreatedAt = DateTime.UtcNow.AddDays(-14),
            UpdatedAt = DateTime.UtcNow.AddDays(-14)
        };
        context.MedicalRecords.Add(record);

        // 7. Seed Lab Results
        var lab1 = new LabResult
        {
            Id = Guid.NewGuid(),
            PatientId = patientProfile.Id,
            DoctorId = doctorProfile.Id,
            TestName = "Total Cholesterol Panel (DEMO)",
            Value = "185",
            Unit = "mg/dL",
            ReferenceRange = "< 200 mg/dL",
            Status = LabResultStatus.Normal,
            TestDate = DateTime.UtcNow.AddDays(-10),
            Notes = "Optimal fasting total cholesterol result."
        };

        var lab2 = new LabResult
        {
            Id = Guid.NewGuid(),
            PatientId = patientProfile.Id,
            DoctorId = doctorProfile.Id,
            TestName = "Fasting Blood Glucose (DEMO)",
            Value = "94",
            Unit = "mg/dL",
            ReferenceRange = "70 - 99 mg/dL",
            Status = LabResultStatus.Normal,
            TestDate = DateTime.UtcNow.AddDays(-10),
            Notes = "Normal fasting glucose within healthy glycemic range."
        };

        var lab3 = new LabResult
        {
            Id = Guid.NewGuid(),
            PatientId = patientProfile.Id,
            DoctorId = doctorProfile.Id,
            TestName = "High-Sensitivity CRP (Inflammation) (DEMO)",
            Value = "1.2",
            Unit = "mg/L",
            ReferenceRange = "< 1.0 mg/L",
            Status = LabResultStatus.Abnormal,
            TestDate = DateTime.UtcNow.AddDays(-10),
            Notes = "Mild borderline elevation. Re-check panel in 3 months."
        };
        context.LabResults.AddRange(lab1, lab2, lab3);

        // 8. Seed Medications
        var med1 = new Medication
        {
            Id = Guid.NewGuid(),
            PatientId = patientProfile.Id,
            Name = "Lisinopril (DEMO)",
            Dosage = "10 mg",
            Frequency = "Once daily in the morning",
            StartDate = DateTime.UtcNow.AddMonths(-3),
            EndDate = null,
            Notes = "Prescribed for blood pressure management.",
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        var med2 = new Medication
        {
            Id = Guid.NewGuid(),
            PatientId = patientProfile.Id,
            Name = "Omega-3 Fish Oil (DEMO)",
            Dosage = "1000 mg",
            Frequency = "Twice daily with meals",
            StartDate = DateTime.UtcNow.AddMonths(-6),
            EndDate = null,
            Notes = "Cardiovascular wellness supplement.",
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };
        context.Medications.AddRange(med1, med2);

        // 9. Seed Medical Document
        var doc = new MedicalDocument
        {
            Id = Guid.NewGuid(),
            PatientId = patientProfile.Id,
            UploadedBy = patientUser.Id,
            FileName = "Cardiology_Echocardiogram_Summary_DEMO.pdf",
            FileType = "application/pdf",
            FileUrl = "/storage/documents/cardio_demo.pdf",
            DocumentType = DocumentType.DischargeSummary,
            ExtractedText = "Echocardiogram Report: Left ventricular ejection fraction 62%. Normal valvular movement. No pericardial effusion.",
            AISummary = "AI Summary: Echocardiogram demonstrates preserved ejection fraction (62%) with healthy cardiac structure and no acute pathology.",
            UploadedAt = DateTime.UtcNow.AddDays(-5)
        };
        context.MedicalDocuments.Add(doc);

        // 10. Seed Medical Articles
        var article1 = new MedicalArticle
        {
            Id = Guid.NewGuid(),
            Title = "Understanding Blood Pressure Baseline Metrics",
            Content = "Regular tracking of systolic and diastolic blood pressure is crucial for early detection of hypertension. Maintaining a balanced diet and regular aerobic exercise can help manage optimal levels.",
            Category = "Cardiology",
            Language = "en",
            Author = "Dr. Alexander Vance",
            PublishedAt = DateTime.UtcNow.AddDays(-20),
            IsPublished = true
        };
        context.MedicalArticles.Add(article1);

        // 11. Seed Notifications
        var notif1 = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = patientUser.Id,
            Title = "Upcoming Appointment Reminder",
            Message = "You have an appointment with Dr. Alexander Vance on " + DateTime.UtcNow.AddDays(2).ToString("MMM dd, yyyy at 10:00 AM") + ".",
            Type = NotificationType.AppointmentReminder,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        context.Notifications.Add(notif1);

        await context.SaveChangesAsync();
    }
}
