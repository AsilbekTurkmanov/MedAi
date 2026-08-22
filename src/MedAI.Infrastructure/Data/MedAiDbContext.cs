using MedAI.Application.Interfaces;
using MedAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedAI.Infrastructure.Data;

public class MedAiDbContext : DbContext, IApplicationDbContext
{
    public MedAiDbContext(DbContextOptions<MedAiDbContext> options) : base(options)
    {
    }

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        // Clinic -> Doctors (One-to-Many)
        modelBuilder.Entity<DoctorProfile>()
            .HasOne(d => d.Clinic)
            .WithMany(c => c.Doctors)
            .HasForeignKey(d => d.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

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

        // MedicalRecords
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

        // MedicalDocuments
        modelBuilder.Entity<MedicalDocument>()
            .HasOne(d => d.Patient)
            .WithMany(p => p.MedicalDocuments)
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // LabResults
        modelBuilder.Entity<LabResult>()
            .HasOne(l => l.Patient)
            .WithMany(p => p.LabResults)
            .HasForeignKey(l => l.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Medications
        modelBuilder.Entity<Medication>()
            .HasOne(m => m.Patient)
            .WithMany(p => p.Medications)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // HealthEvents
        modelBuilder.Entity<HealthEvent>()
            .HasOne(h => h.Patient)
            .WithMany(p => p.HealthEvents)
            .HasForeignKey(h => h.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // AISession -> AIMessage
        modelBuilder.Entity<AIMessage>()
            .HasOne(m => m.AISession)
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.AISessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
