namespace MedAI.Domain.Enums;

public enum UserRole
{
    Patient = 1,
    Doctor = 2,
    ClinicAdmin = 3,
    SuperAdmin = 4
}

public enum AppointmentStatus
{
    Pending = 1,
    Confirmed = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5
}

public enum LabResultStatus
{
    Normal = 1,
    Abnormal = 2,
    Critical = 3,
    Pending = 4
}

public enum DocumentType
{
    General = 1,
    BloodTest = 2,
    XRay = 3,
    MRI = 4,
    Prescription = 5,
    DischargeSummary = 6,
    LabReport = 7,
    DoctorNote = 8
}

public enum HealthEventType
{
    Symptom = 1,
    Diagnosis = 2,
    Vaccination = 3,
    Surgery = 4,
    Allergy = 5,
    Measurement = 6,
    Note = 7,
    Hospitalization = 8,
    ChronicCondition = 9
}

public enum SafetyLevel
{
    Safe = 1,
    Precaution = 2,
    EmergencyWarning = 3
}

public enum NotificationType
{
    AppointmentReminder = 1,
    MedicationReminder = 2,
    DoctorMessage = 3,
    DocumentProcessed = 4,
    FollowUp = 5,
    System = 6,
    AppointmentCancelled = 7,
    AppointmentConfirmed = 8,
    HealthAlert = 9
}

public enum TriageLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Emergency = 4
}

public enum AllergySeverity
{
    Mild = 1,
    Moderate = 2,
    Severe = 3,
    LifeThreatening = 4
}

public enum MedicationStatus
{
    Active = 1,
    Completed = 2,
    Discontinued = 3,
    OnHold = 4
}

public enum ConsentScope
{
    FullProfile = 1,
    MedicalHistory = 2,
    Documents = 3,
    Medications = 4,
    LabResults = 5,
    Appointments = 6,
    Allergies = 7,
    Timeline = 8
}

public enum ChronicConditionStatus
{
    Active = 1,
    InRemission = 2,
    Resolved = 3,
    Monitoring = 4
}

public enum FamilyRelationship
{
    Self = 1,
    Child = 2,
    Parent = 3,
    Spouse = 4,
    Sibling = 5,
    Other = 6
}

public enum NotificationPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

public enum DayOfWeekEnum
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7
}
