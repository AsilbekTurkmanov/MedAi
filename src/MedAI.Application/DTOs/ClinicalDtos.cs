using System;
using System.Collections.Generic;
using MedAI.Domain.Enums;

namespace MedAI.Application.DTOs.Clinical;

// PATIENT DTOs
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
}

public class TimelineItemDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty; // Appointment, LabResult, Document, Medication, HealthEvent, MedicalRecord
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string BadgeColor { get; set; } = "blue";
    public string DetailsUrl { get; set; } = string.Empty;
}

// DOCTOR DTOs
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
    public List<string> AvailableSlots { get; set; } = new();
}

// CLINIC DTOs
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

// APPOINTMENT DTOs
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

// MEDICAL RECORD DTOs
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

// MEDICAL DOCUMENT DTOs
public class MedicalDocumentDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid UploadedBy { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string ExtractedText { get; set; } = string.Empty;
    public string AISummary { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

// LAB RESULT DTOs
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

// MEDICATION DTOs
public class MedicationDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
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
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}

// HEALTH EVENT DTOs
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

// FAMILY MEMBER DTOs
public class FamilyMemberDto
{
    public Guid Id { get; set; }
    public Guid OwnerPatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Permissions { get; set; } = string.Empty;
}

public class CreateFamilyMemberDto
{
    public string Name { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Permissions { get; set; } = "ViewOnly";
}

// NOTIFICATION DTOs
public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ADMIN DTOs
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
