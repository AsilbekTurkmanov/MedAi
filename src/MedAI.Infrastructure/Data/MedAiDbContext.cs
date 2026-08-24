using MedAI.Application.Interfaces;
using MedAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedAI.Infrastructure.Data;

public class MedAiDbContext : DbContext, IApplicationDbContext
{
    public MedAiDbContext(DbContextOptions<MedAiDbContext> options) : base(options)
    {
    }

    // Existing entities
    public DbSet<User> Users => Set<User>();
    public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<MedicalDocument> MedicalDocuments => Set<MedicalDocument>();
    public DbSet<LabResult> LabResults => Set<LabResult>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<HealthEvent> HealthEvents => Set<HealthEvent>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AISession> AISessions => Set<AISession>();
    public DbSet<AIMessage> AIMessages => Set<AIMessage>();
    public DbSet<DoctorNote> DoctorNotes => Set<DoctorNote>();
    public DbSet<MedicalArticle> MedicalArticles => Set<MedicalArticle>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // New entities
    public DbSet<Allergy> Allergies => Set<Allergy>();
    public DbSet<ChronicCondition> ChronicConditions => Set<ChronicCondition>();
    public DbSet<Vaccination> Vaccinations => Set<Vaccination>();
    public DbSet<DataConsent> DataConsents => Set<DataConsent>();
    public DbSet<DataAccessLog> DataAccessLogs => Set<DataAccessLog>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<DoctorLeave> DoctorLeaves => Set<DoctorLeave>();
    public DbSet<AIHandoffSummary> AIHandoffSummaries => Set<AIHandoffSummary>();
    public DbSet<QrHealthToken> QrHealthTokens => Set<QrHealthToken>();
    public DbSet<MedicalTermMapping> MedicalTermMappings => Set<MedicalTermMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== USER RELATIONSHIPS =====

        // User -> PatientProfile (One-to-One)
        modelBuilder.Entity<User>()
            .HasOne(u => u.PatientProfile)
            .WithOne(p => p.User)
            .HasForeignKey<PatientProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> DoctorProfile (One-to-One)
        modelBuilder.Entity<User>()
            .HasOne(u => u.DoctorProfile)
            .WithOne(d => d.User)
            .HasForeignKey<DoctorProfile>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== CLINIC RELATIONSHIPS =====

        // Clinic -> Doctors (One-to-Many)
        modelBuilder.Entity<DoctorProfile>()
            .HasOne(d => d.Clinic)
            .WithMany(c => c.Doctors)
            .HasForeignKey(d => d.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== APPOINTMENT RELATIONSHIPS =====

        // Patient -> Appointments
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Doctor -> Appointments
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Clinic -> Appointments
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Clinic)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== MEDICAL RECORDS =====

        modelBuilder.Entity<MedicalRecord>()
            .HasOne(m => m.Patient)
            .WithMany(p => p.MedicalRecords)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MedicalRecord>()
            .HasOne(m => m.Doctor)
            .WithMany(d => d.MedicalRecords)
            .HasForeignKey(m => m.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== MEDICAL DOCUMENTS =====

        modelBuilder.Entity<MedicalDocument>()
            .HasOne(d => d.Patient)
            .WithMany(p => p.MedicalDocuments)
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== LAB RESULTS =====

        modelBuilder.Entity<LabResult>()
            .HasOne(l => l.Patient)
            .WithMany(p => p.LabResults)
            .HasForeignKey(l => l.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LabResult>()
            .HasOne(l => l.Doctor)
            .WithMany(d => d.LabResults)
            .HasForeignKey(l => l.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== MEDICATIONS =====

        modelBuilder.Entity<Medication>()
            .HasOne(m => m.Patient)
            .WithMany(p => p.Medications)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Medication>()
            .HasOne(m => m.PrescribedByDoctor)
            .WithMany()
            .HasForeignKey(m => m.PrescribedByDoctorId)
            .OnDelete(DeleteBehavior.SetNull);

        // ===== HEALTH EVENTS =====

        modelBuilder.Entity<HealthEvent>()
            .HasOne(h => h.Patient)
            .WithMany(p => p.HealthEvents)
            .HasForeignKey(h => h.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== FAMILY MEMBERS =====

        modelBuilder.Entity<FamilyMember>()
            .HasOne(f => f.OwnerPatient)
            .WithMany(p => p.FamilyMembers)
            .HasForeignKey(f => f.OwnerPatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== AI SESSION -> AI MESSAGE =====

        modelBuilder.Entity<AIMessage>()
            .HasOne(m => m.AISession)
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.AISessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== DOCTOR NOTES =====

        modelBuilder.Entity<DoctorNote>()
            .HasOne(n => n.Doctor)
            .WithMany(d => d.DoctorNotes)
            .HasForeignKey(n => n.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DoctorNote>()
            .HasOne(n => n.Patient)
            .WithMany(p => p.DoctorNotes)
            .HasForeignKey(n => n.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== PRESCRIPTIONS =====

        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.Patient)
            .WithMany(pat => pat.Prescriptions)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.Doctor)
            .WithMany(d => d.Prescriptions)
            .HasForeignKey(p => p.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== NEW ENTITY RELATIONSHIPS =====

        // Allergies
        modelBuilder.Entity<Allergy>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Allergies)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // ChronicConditions
        modelBuilder.Entity<ChronicCondition>()
            .HasOne(c => c.Patient)
            .WithMany(p => p.ChronicConditions)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Vaccinations
        modelBuilder.Entity<Vaccination>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.Vaccinations)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // DataConsent
        modelBuilder.Entity<DataConsent>()
            .HasOne(c => c.Patient)
            .WithMany(p => p.DataConsents)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DataConsent>()
            .HasOne(c => c.GrantedToUser)
            .WithMany()
            .HasForeignKey(c => c.GrantedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // DataAccessLog
        modelBuilder.Entity<DataAccessLog>()
            .HasOne(l => l.Patient)
            .WithMany(p => p.DataAccessLogs)
            .HasForeignKey(l => l.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DataAccessLog>()
            .HasOne(l => l.AccessedByUser)
            .WithMany()
            .HasForeignKey(l => l.AccessedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // DoctorSchedule
        modelBuilder.Entity<DoctorSchedule>()
            .HasOne(s => s.Doctor)
            .WithMany(d => d.Schedules)
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // DoctorLeave
        modelBuilder.Entity<DoctorLeave>()
            .HasOne(l => l.Doctor)
            .WithMany(d => d.Leaves)
            .HasForeignKey(l => l.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // AIHandoffSummary
        modelBuilder.Entity<AIHandoffSummary>()
            .HasOne(h => h.AISession)
            .WithMany()
            .HasForeignKey(h => h.AISessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AIHandoffSummary>()
            .HasOne(h => h.Patient)
            .WithMany()
            .HasForeignKey(h => h.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // QrHealthToken
        modelBuilder.Entity<QrHealthToken>()
            .HasOne(q => q.Patient)
            .WithMany()
            .HasForeignKey(q => q.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== INDEXES =====

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.CreatedAt);

        modelBuilder.Entity<PatientProfile>().HasIndex(p => p.UserId).IsUnique();
        modelBuilder.Entity<DoctorProfile>().HasIndex(d => d.UserId).IsUnique();
        modelBuilder.Entity<DoctorProfile>().HasIndex(d => d.ClinicId);

        modelBuilder.Entity<Appointment>().HasIndex(a => a.PatientId);
        modelBuilder.Entity<Appointment>().HasIndex(a => a.DoctorId);
        modelBuilder.Entity<Appointment>().HasIndex(a => a.ClinicId);
        modelBuilder.Entity<Appointment>().HasIndex(a => a.AppointmentDate);
        modelBuilder.Entity<Appointment>().HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.StartTime });

        modelBuilder.Entity<MedicalRecord>().HasIndex(m => m.PatientId);
        modelBuilder.Entity<MedicalRecord>().HasIndex(m => m.DoctorId);
        modelBuilder.Entity<MedicalRecord>().HasIndex(m => m.CreatedAt);

        modelBuilder.Entity<MedicalDocument>().HasIndex(d => d.PatientId);
        modelBuilder.Entity<LabResult>().HasIndex(l => l.PatientId);
        modelBuilder.Entity<LabResult>().HasIndex(l => l.TestDate);

        modelBuilder.Entity<Medication>().HasIndex(m => m.PatientId);
        modelBuilder.Entity<HealthEvent>().HasIndex(h => h.PatientId);
        modelBuilder.Entity<HealthEvent>().HasIndex(h => h.EventDate);

        modelBuilder.Entity<Notification>().HasIndex(n => n.UserId);
        modelBuilder.Entity<Notification>().HasIndex(n => n.CreatedAt);
        modelBuilder.Entity<Notification>().HasIndex(n => new { n.UserId, n.IsRead });

        modelBuilder.Entity<AISession>().HasIndex(s => s.UserId);
        modelBuilder.Entity<AIMessage>().HasIndex(m => m.AISessionId);

        modelBuilder.Entity<AuditLog>().HasIndex(a => a.CreatedAt);
        modelBuilder.Entity<AuditLog>().HasIndex(a => a.UserId);

        modelBuilder.Entity<Allergy>().HasIndex(a => a.PatientId);
        modelBuilder.Entity<ChronicCondition>().HasIndex(c => c.PatientId);
        modelBuilder.Entity<Vaccination>().HasIndex(v => v.PatientId);

        modelBuilder.Entity<DataConsent>().HasIndex(c => c.PatientId);
        modelBuilder.Entity<DataConsent>().HasIndex(c => c.GrantedToUserId);
        modelBuilder.Entity<DataAccessLog>().HasIndex(l => l.PatientId);
        modelBuilder.Entity<DataAccessLog>().HasIndex(l => l.AccessedAt);

        modelBuilder.Entity<DoctorSchedule>().HasIndex(s => s.DoctorId);
        modelBuilder.Entity<DoctorLeave>().HasIndex(l => l.DoctorId);
        modelBuilder.Entity<DoctorLeave>().HasIndex(l => new { l.DoctorId, l.StartDate, l.EndDate });

        modelBuilder.Entity<MedicalTermMapping>().HasIndex(m => m.EnglishTerm);
        modelBuilder.Entity<MedicalTermMapping>().HasIndex(m => m.UzbekTerm);
    }
}
