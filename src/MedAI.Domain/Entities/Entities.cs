using System;
using System.Collections.Generic;
using MedAI.Domain.Enums;

namespace MedAI.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = "NotSpecified";
    public UserRole Role { get; set; } = UserRole.Patient;
    public string PreferredLanguage { get; set; } = "en";
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation
    public PatientProfile? PatientProfile { get; set; }
    public DoctorProfile? DoctorProfile { get; set; }
    public ICollection<AISession> AISessions { get; set; } = new List<AISession>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

public class PatientProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string BloodType { get; set; } = "O+";
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public ICollection<MedicalDocument> MedicalDocuments { get; set; } = new List<MedicalDocument>();
    public ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();
    public ICollection<Medication> Medications { get; set; } = new List<Medication>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<HealthEvent> HealthEvents { get; set; } = new List<HealthEvent>();
    public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
    public ICollection<DoctorNote> DoctorNotes { get; set; } = new List<DoctorNote>();
    public ICollection<Allergy> Allergies { get; set; } = new List<Allergy>();
    public ICollection<ChronicCondition> ChronicConditions { get; set; } = new List<ChronicCondition>();
    public ICollection<Vaccination> Vaccinations { get; set; } = new List<Vaccination>();
    public ICollection<DataConsent> DataConsents { get; set; } = new List<DataConsent>();
    public ICollection<DataAccessLog> DataAccessLogs { get; set; } = new List<DataAccessLog>();
}

public class Clinic
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<DoctorProfile> Doctors { get; set; } = new List<DoctorProfile>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

public class DoctorProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Specialization { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string Bio { get; set; } = string.Empty;
    public Guid ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    public bool IsVerified { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<DoctorNote> DoctorNotes { get; set; } = new List<DoctorNote>();
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
    public ICollection<DoctorLeave> Leaves { get; set; } = new List<DoctorLeave>();
}

public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public DoctorProfile Doctor { get; set; } = null!;
    public Guid ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string Reason { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class MedicalRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public DoctorProfile Doctor { get; set; } = null!;
    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiagnosisNotes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class MedicalDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public Guid UploadedBy { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DocumentType DocumentType { get; set; } = DocumentType.General;
    public string ExtractedText { get; set; } = string.Empty;
    public string AISummary { get; set; } = string.Empty;
    public bool IsProcessed { get; set; } = false;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

public class LabResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public DoctorProfile Doctor { get; set; } = null!;
    public string TestName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string ReferenceRange { get; set; } = string.Empty;
    public LabResultStatus Status { get; set; } = LabResultStatus.Normal;
    public DateTime TestDate { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;
}

public class Medication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public Guid? PrescribedByDoctorId { get; set; }
    public DoctorProfile? PrescribedByDoctor { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public MedicationStatus Status { get; set; } = MedicationStatus.Active;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Prescription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public DoctorProfile Doctor { get; set; } = null!;
    public Guid MedicationId { get; set; }
    public Medication Medication { get; set; } = null!;
    public string Instructions { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Active";
}

public class HealthEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public HealthEventType Type { get; set; } = HealthEventType.Symptom;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class FamilyMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerPatientId { get; set; }
    public PatientProfile OwnerPatient { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public FamilyRelationship Relationship { get; set; } = FamilyRelationship.Other;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = "NotSpecified";
    public string BloodType { get; set; } = string.Empty;
    public Guid? LinkedPatientProfileId { get; set; }
    public string Permissions { get; set; } = "ViewOnly";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.System;
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AISession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Title { get; set; } = "AI Health Session";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AIMessage> Messages { get; set; } = new List<AIMessage>();
}

public class AIMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AISessionId { get; set; }
    public AISession AISession { get; set; } = null!;
    public string Role { get; set; } = "user"; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DoctorNote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DoctorId { get; set; }
    public DoctorProfile Doctor { get; set; } = null!;
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class MedicalArticle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public bool IsPublished { get; set; } = true;
}

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string IpAddress { get; set; } = "127.0.0.1";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ===== NEW ENTITIES (Phase 2-4) =====

public class Allergy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public AllergySeverity Severity { get; set; } = AllergySeverity.Mild;
    public string Reaction { get; set; } = string.Empty;
    public DateTime? DiagnosedDate { get; set; }
    public string Source { get; set; } = "Patient"; // Patient, Doctor, System
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ChronicCondition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public DateTime? DiagnosedDate { get; set; }
    public ChronicConditionStatus Status { get; set; } = ChronicConditionStatus.Active;
    public string Notes { get; set; } = string.Empty;
    public string Source { get; set; } = "Patient";
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Vaccination
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public DateTime DateAdministered { get; set; }
    public string Provider { get; set; } = string.Empty;
    public int DoseNumber { get; set; } = 1;
    public string Notes { get; set; } = string.Empty;
    public string Source { get; set; } = "Patient";
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DataConsent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public Guid GrantedToUserId { get; set; }
    public User GrantedToUser { get; set; } = null!;
    public ConsentScope Scope { get; set; } = ConsentScope.FullProfile;
    public bool IsActive { get; set; } = true;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class DataAccessLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public Guid AccessedByUserId { get; set; }
    public User AccessedByUser { get; set; } = null!;
    public string AccessReason { get; set; } = string.Empty;
    public string DataScope { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime AccessedAt { get; set; } = DateTime.UtcNow;
}

public class DoctorSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DoctorId { get; set; }
    public DoctorProfile Doctor { get; set; } = null!;
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DoctorLeave
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DoctorId { get; set; }
    public DoctorProfile Doctor { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AIHandoffSummary
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AISessionId { get; set; }
    public AISession AISession { get; set; } = null!;
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }
    public string MainComplaint { get; set; } = string.Empty;
    public string SymptomsSummary { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string RelevantHistory { get; set; } = string.Empty;
    public string CurrentMedications { get; set; } = string.Empty;
    public string Allergies { get; set; } = string.Empty;
    public string TriageCategory { get; set; } = string.Empty;
    public string ConversationSummary { get; set; } = string.Empty;
    public string QuestionsForDoctor { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class QrHealthToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public PatientProfile Patient { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;
    public ConsentScope Scope { get; set; } = ConsentScope.FullProfile;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class MedicalTermMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UzbekTerm { get; set; } = string.Empty;
    public string RussianTerm { get; set; } = string.Empty;
    public string EnglishTerm { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
