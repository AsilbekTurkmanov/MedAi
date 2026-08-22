using System.Threading;
using System.Threading.Tasks;
using MedAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedAI.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<PatientProfile> PatientProfiles { get; }
    DbSet<DoctorProfile> DoctorProfiles { get; }
    DbSet<Clinic> Clinics { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<MedicalRecord> MedicalRecords { get; }
    DbSet<MedicalDocument> MedicalDocuments { get; }
    DbSet<LabResult> LabResults { get; }
    DbSet<Medication> Medications { get; }
    DbSet<Prescription> Prescriptions { get; }
    DbSet<HealthEvent> HealthEvents { get; }
    DbSet<FamilyMember> FamilyMembers { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AISession> AISessions { get; }
    DbSet<AIMessage> AIMessages { get; }
    DbSet<DoctorNote> DoctorNotes { get; }
    DbSet<MedicalArticle> MedicalArticles { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
