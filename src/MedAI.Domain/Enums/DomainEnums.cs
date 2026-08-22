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
    DischargeSummary = 6
}

public enum HealthEventType
{
    Symptom = 1,
    Diagnosis = 2,
    Vaccination = 3,
    Surgery = 4,
    Allergy = 5,
    Measurement = 6,
    Note = 7
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
    System = 6
}
