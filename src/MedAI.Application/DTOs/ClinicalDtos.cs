using System;
using System.Collections.Generic;
using MedAI.Domain.Enums;

namespace MedAI.Application.DTOs.Clinical;

// ===== PATIENT DTOs =====

public class PatientProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdatePatientProfileDto
{
    public string BloodType { get; set; } = string.Empty;
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class HealthPassportDto
{
    public Guid PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }
    public string BloodType { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public List<MedicationSummaryDto> ActiveMedications { get; set; } = new();
    public List<LabSummaryDto> RecentLabResults { get; set; } = new();
    public List<HealthEventDto> ActiveConditions { get; set; } = new();
    public List<AllergyDto> Allergies { get; set; } = new();
    public List<ChronicConditionDto> ChronicConditions { get; set; } = new();
    public List<VaccinationDto> Vaccinations { get; set; } = new();
}

public class TimelineItemDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string BadgeColor { get; set; } = "blue";
    public string DetailsUrl { get; set; } = string.Empty;
}

// ===== ALLERGY DTOs (Phase 2) =====

public class AllergyDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AllergySeverity Severity { get; set; }
    public string Reaction { get; set; } = string.Empty;
    public DateTime? DiagnosedDate { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateAllergyDto
{
    public string Name { get; set; } = string.Empty;
    public AllergySeverity Severity { get; set; } = AllergySeverity.Mild;
    public string Reaction { get; set; } = string.Empty;
    public DateTime? DiagnosedDate { get; set; }
}

// ===== CHRONIC CONDITION DTOs (Phase 2) =====

public class ChronicConditionDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? DiagnosedDate { get; set; }
    public ChronicConditionStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateChronicConditionDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime? DiagnosedDate { get; set; }
    public ChronicConditionStatus Status { get; set; } = ChronicConditionStatus.Active;
    public string Notes { get; set; } = string.Empty;
}

// ===== VACCINATION DTOs (Phase 2) =====

public class VaccinationDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DateAdministered { get; set; }
    public string Provider { get; set; } = string.Empty;
    public int DoseNumber { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateVaccinationDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime DateAdministered { get; set; }
    public string Provider { get; set; } = string.Empty;
    public int DoseNumber { get; set; } = 1;
    public string Notes { get; set; } = string.Empty;
}

// ===== CONSENT DTOs (Phase 3) =====

public class DataConsentDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid GrantedToUserId { get; set; }
    public string GrantedToUserName { get; set; } = string.Empty;
    public string GrantedToUserRole { get; set; } = string.Empty;
    public ConsentScope Scope { get; set; }
    public bool IsActive { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class GrantConsentDto
{
    public Guid GrantToUserId { get; set; }
    public ConsentScope Scope { get; set; } = ConsentScope.FullProfile;
    public DateTime? ExpiresAt { get; set; }
}

public class DataAccessLogDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid AccessedByUserId { get; set; }
    public string AccessedByUserName { get; set; } = string.Empty;
    public string AccessReason { get; set; } = string.Empty;
    public string DataScope { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime AccessedAt { get; set; }
}

// ===== DOCTOR SCHEDULING DTOs (Phase 4) =====

public class DoctorScheduleDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int SlotDurationMinutes { get; set; }
    public bool IsActive { get; set; }
}

public class CreateDoctorScheduleDto
{
    public DayOfWeekEnum DayOfWeek { get; set; }
    public string StartTime { get; set; } = "09:00";
    public string EndTime { get; set; } = "17:00";
    public int SlotDurationMinutes { get; set; } = 30;
}

public class DoctorLeaveDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CreateDoctorLeaveDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class AvailableSlotDto
{
    public DateTime Date { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}

// ===== DOCTOR DTOs =====

public class DoctorProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string Bio { get; set; } = string.Empty;
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateDoctorProfileDto
{
    public string Specialization { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
}

public class DoctorAvailabilityDto
{
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public List<AvailableSlotDto> AvailableSlots { get; set; } = new();
}

// ===== CLINIC DTOs =====

public class ClinicDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public int DoctorCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ===== APPOINTMENT DTOs =====

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialization { get; set; } = string.Empty;
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateAppointmentDto
{
    public Guid DoctorId { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string StartTime { get; set; } = "09:00:00";
    public string EndTime { get; set; } = "09:30:00";
    public string Reason { get; set; } = string.Empty;
}

public class UpdateAppointmentDto
{
    public AppointmentStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
}

// ===== MEDICAL RECORD DTOs =====

public class MedicalRecordDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public Guid? AppointmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiagnosisNotes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateMedicalRecordDto
{
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiagnosisNotes { get; set; } = string.Empty;
}

// ===== MEDICAL DOCUMENT DTOs =====

public class MedicalDocumentDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid UploadedBy { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DocumentType DocumentType { get; set; }
    public string ExtractedText { get; set; } = string.Empty;
    public string AISummary { get; set; } = string.Empty;
    public bool IsProcessed { get; set; }
    public DateTime UploadedAt { get; set; }
}

// ===== LAB RESULT DTOs =====

public class LabResultDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string ReferenceRange { get; set; } = string.Empty;
    public LabResultStatus Status { get; set; }
    public DateTime TestDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CreateLabResultDto
{
    public Guid PatientId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string ReferenceRange { get; set; } = string.Empty;
    public LabResultStatus Status { get; set; } = LabResultStatus.Normal;
    public DateTime TestDate { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;
}

public class LabSummaryDto
{
    public string TestName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public LabResultStatus Status { get; set; }
    public DateTime TestDate { get; set; }
}

public class LabTrendDto
{
    public string TestName { get; set; } = string.Empty;
    public List<LabTrendPointDto> Points { get; set; } = new();
}

public class LabTrendPointDto
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
}

// ===== MEDICATION DTOs =====

public class MedicationDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid? PrescribedByDoctorId { get; set; }
    public string PrescribedByDoctorName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public MedicationStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class MedicationSummaryDto
{
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
}

public class CreateMedicationDto
{
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}

// ===== HEALTH EVENT DTOs =====

public class HealthEventDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public HealthEventType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateHealthEventDto
{
    public HealthEventType Type { get; set; } = HealthEventType.Symptom;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; } = DateTime.UtcNow;
}

// ===== FAMILY MEMBER DTOs =====

public class FamilyMemberDto
{
    public Guid Id { get; set; }
    public Guid OwnerPatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public FamilyRelationship Relationship { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public Guid? LinkedPatientProfileId { get; set; }
    public string Permissions { get; set; } = string.Empty;
}

public class CreateFamilyMemberDto
{
    public string Name { get; set; } = string.Empty;
    public FamilyRelationship Relationship { get; set; } = FamilyRelationship.Other;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = "NotSpecified";
    public string BloodType { get; set; } = string.Empty;
    public string Permissions { get; set; } = "ViewOnly";
}

// ===== NOTIFICATION DTOs =====

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ===== ADMIN DTOs =====

public class AdminDashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalClinics { get; set; }
    public int TotalAppointments { get; set; }
    public int TotalAiSessions { get; set; }
    public int PendingAppointments { get; set; }
    public int CompletedAppointments { get; set; }
}

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserManageDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ===== AI HANDOFF DTOs (Phase 7) =====

public class AIHandoffSummaryDto
{
    public Guid Id { get; set; }
    public Guid AISessionId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid? AppointmentId { get; set; }
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
    public DateTime CreatedAt { get; set; }
}

// ===== QR HEALTH PASSPORT DTOs (Phase 11) =====

public class QrHealthTokenDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public ConsentScope Scope { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GenerateQrTokenDto
{
    public ConsentScope Scope { get; set; } = ConsentScope.FullProfile;
    public int ExpiresInMinutes { get; set; } = 30;
}

// ===== SEARCH DTOs (Phase 16) =====

public class SearchResultDto
{
    public string Type { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
