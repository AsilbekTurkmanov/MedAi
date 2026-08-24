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
            PrescribedByDoctorId = doctorProfile.Id,
            Name = "Lisinopril (DEMO)",
            Dosage = "10 mg",
            Frequency = "Once daily in the morning",
            Instructions = "Take on an empty stomach. Monitor blood pressure regularly.",
            Status = MedicationStatus.Active,
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
            Instructions = "Take with food to improve absorption.",
            Status = MedicationStatus.Active,
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
            FileSizeBytes = 245760,
            DocumentType = DocumentType.DischargeSummary,
            ExtractedText = "Echocardiogram Report: Left ventricular ejection fraction 62%. Normal valvular movement. No pericardial effusion.",
            AISummary = "AI Summary: Echocardiogram demonstrates preserved ejection fraction (62%) with healthy cardiac structure and no acute pathology.",
            IsProcessed = true,
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
            Priority = NotificationPriority.Normal,
            ActionUrl = "/appointments",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        context.Notifications.Add(notif1);

        // 12. Seed Allergies (Phase 2)
        context.Allergies.AddRange(
            new Allergy
            {
                Id = Guid.NewGuid(),
                PatientId = patientProfile.Id,
                Name = "Penicillin",
                Severity = AllergySeverity.Moderate,
                Reaction = "Skin rash, mild hives",
                DiagnosedDate = new DateTime(2018, 3, 15),
                Source = "Doctor",
                CreatedByUserId = doctorUser.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Allergy
            {
                Id = Guid.NewGuid(),
                PatientId = patientProfile.Id,
                Name = "Shellfish",
                Severity = AllergySeverity.Mild,
                Reaction = "Mild stomach discomfort",
                DiagnosedDate = new DateTime(2020, 7, 10),
                Source = "Patient",
                CreatedByUserId = patientUser.Id,
                CreatedAt = DateTime.UtcNow
            }
        );

        // 13. Seed Chronic Conditions (Phase 2)
        context.ChronicConditions.Add(new ChronicCondition
        {
            Id = Guid.NewGuid(),
            PatientId = patientProfile.Id,
            Name = "Essential Hypertension (Stage 1)",
            DiagnosedDate = new DateTime(2023, 1, 20),
            Status = ChronicConditionStatus.Active,
            Notes = "Managed with Lisinopril 10mg daily. Blood pressure target: < 130/80 mmHg.",
            Source = "Doctor",
            CreatedByUserId = doctorUser.Id,
            CreatedAt = DateTime.UtcNow
        });

        // 14. Seed Vaccinations (Phase 2)
        context.Vaccinations.AddRange(
            new Vaccination
            {
                Id = Guid.NewGuid(),
                PatientId = patientProfile.Id,
                Name = "COVID-19 (Pfizer-BioNTech)",
                DateAdministered = new DateTime(2021, 4, 15),
                Provider = "City Health Department",
                DoseNumber = 2,
                Notes = "Second dose completed.",
                Source = "Patient",
                CreatedAt = DateTime.UtcNow
            },
            new Vaccination
            {
                Id = Guid.NewGuid(),
                PatientId = patientProfile.Id,
                Name = "Influenza (Seasonal Flu)",
                DateAdministered = new DateTime(2025, 10, 5),
                Provider = "MedAI Central Clinic",
                DoseNumber = 1,
                Notes = "Annual flu vaccination.",
                Source = "Doctor",
                CreatedAt = DateTime.UtcNow
            }
        );

        // 15. Seed Health Events
        context.HealthEvents.AddRange(
            new HealthEvent
            {
                Id = Guid.NewGuid(),
                PatientId = patientProfile.Id,
                Type = HealthEventType.Diagnosis,
                Title = "Essential Hypertension Diagnosed",
                Description = "Stage 1 hypertension diagnosed during routine checkup. BP: 142/88 mmHg.",
                EventDate = new DateTime(2023, 1, 20),
                CreatedAt = DateTime.UtcNow
            },
            new HealthEvent
            {
                Id = Guid.NewGuid(),
                PatientId = patientProfile.Id,
                Type = HealthEventType.Symptom,
                Title = "Mild evening headaches",
                Description = "Patient reported recurring mild headaches in the evening, potentially related to screen time or blood pressure.",
                EventDate = DateTime.UtcNow.AddDays(-7),
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            }
        );

        // 16. Seed Doctor Schedule (Phase 4)
        var daysOfWeek = new[] { DayOfWeekEnum.Monday, DayOfWeekEnum.Tuesday, DayOfWeekEnum.Wednesday, DayOfWeekEnum.Thursday, DayOfWeekEnum.Friday };
        foreach (var day in daysOfWeek)
        {
            context.DoctorSchedules.Add(new DoctorSchedule
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorProfile.Id,
                DayOfWeek = day,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(17, 0, 0),
                SlotDurationMinutes = 30,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // 17. Seed Doctor Note
        context.DoctorNotes.Add(new DoctorNote
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorProfile.Id,
            PatientId = patientProfile.Id,
            AppointmentId = appt2.Id,
            Content = "Patient is compliant with medication regimen. BP trending toward target. Recommend continued monitoring and follow-up lipid panel in 3 months.",
            CreatedAt = DateTime.UtcNow.AddDays(-14),
            UpdatedAt = DateTime.UtcNow.AddDays(-14)
        });

        // 18. Seed Medical Term Mappings (Phase 13)
        context.MedicalTermMappings.AddRange(
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "bosh og'rig'i", RussianTerm = "головная боль", EnglishTerm = "headache", Category = "Symptom" },
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "isitma", RussianTerm = "температура", EnglishTerm = "fever", Category = "Symptom" },
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "yo'tal", RussianTerm = "кашель", EnglishTerm = "cough", Category = "Symptom" },
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "ko'krak og'rig'i", RussianTerm = "боль в груди", EnglishTerm = "chest pain", Category = "Symptom" },
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "nafas qisishi", RussianTerm = "одышка", EnglishTerm = "shortness of breath", Category = "Symptom" },
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "qorin og'rig'i", RussianTerm = "боль в животе", EnglishTerm = "abdominal pain", Category = "Symptom" },
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "bosh aylanishi", RussianTerm = "головокружение", EnglishTerm = "dizziness", Category = "Symptom" },
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "holsizlik", RussianTerm = "слабость", EnglishTerm = "fatigue", Category = "Symptom" },
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "yurak urishi tezlashishi", RussianTerm = "учащённое сердцебиение", EnglishTerm = "rapid heartbeat", Category = "Symptom" },
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "qon bosimi", RussianTerm = "артериальное давление", EnglishTerm = "blood pressure", Category = "Vital Sign" },
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "qandli diabet", RussianTerm = "сахарный диабет", EnglishTerm = "diabetes mellitus", Category = "Condition" },
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "gipertoniya", RussianTerm = "гипертония", EnglishTerm = "hypertension", Category = "Condition" },
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "allergiya", RussianTerm = "аллергия", EnglishTerm = "allergy", Category = "Condition" },
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "emlash", RussianTerm = "вакцинация", EnglishTerm = "vaccination", Category = "Procedure" },
            new MedicalTermMapping { Id = Guid.NewGuid(), UzbekTerm = "qon tahlili", RussianTerm = "анализ крови", EnglishTerm = "blood test", Category = "Procedure" }
        );

        await context.SaveChangesAsync();
    }
}
