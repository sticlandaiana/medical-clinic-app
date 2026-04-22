using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MedicalClinic.Models;

namespace MedicalClinic.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ----------------------------------------------------------------
        // DbSet-uri — TOATE modelele trebuie sa fie aici
        // ----------------------------------------------------------------
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Nurse> Nurses { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecordEntry> MedicalRecordEntries { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<ExternalDocument> ExternalDocuments { get; set; }
        public DbSet<ConsultationType> ConsultationTypes { get; set; }
        public DbSet<Speciality> Specialities { get; set; }
        public DbSet<DoctorSpeciality> DoctorSpecialities { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----------------------------------------------------------------
            // PATIENT — relatii cu Restrict pentru a evita cascade paths
            // ----------------------------------------------------------------
            modelBuilder.Entity<Allergy>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Allergies)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExternalDocument>()
                .HasOne(d => d.Patient)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalRecordEntry>()
                .HasOne(m => m.Patient)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------------------------------------------
            // APPOINTMENT — are 5 FK-uri, toate trebuie Restrict
            // altfel SQL Server arunca "multiple cascade paths"
            // ----------------------------------------------------------------
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Nurse)
                .WithMany(n => n.Appointments)
                .HasForeignKey(a => a.NurseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Room)
                .WithMany(r => r.Appointments)
                .HasForeignKey(a => a.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.ConsultationType)
                .WithMany(c => c.Appointments)
                .HasForeignKey(a => a.ConsultationTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------------------------------------------
            // MEDICAL RECORD ENTRY — are 3 FK-uri, toate Restrict
            // ----------------------------------------------------------------
            modelBuilder.Entity<MedicalRecordEntry>()
                .HasOne(m => m.Doctor)
                .WithMany(d => d.MedicalRecords)
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalRecordEntry>()
                .HasOne(m => m.Appointment)
                .WithMany(a => a.MedicalRecords)
                .HasForeignKey(m => m.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------------------------------------------
            // REVIEW — FK catre Doctor
            // ----------------------------------------------------------------
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Doctor)
                .WithMany(d => d.Reviews)
                .HasForeignKey(r => r.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------------------------------------------
            // DOCTOR SPECIALITY — relatie many-to-many Doctor <-> Speciality
            // ----------------------------------------------------------------
            modelBuilder.Entity<DoctorSpeciality>()
                .HasOne(ds => ds.Doctor)
                .WithMany(d => d.DoctorSpecialities)
                .HasForeignKey(ds => ds.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorSpeciality>()
                .HasOne(ds => ds.Speciality)
                .WithMany(s => s.DoctorSpecialities)
                .HasForeignKey(ds => ds.SpecialityId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------------------------------------------
            // CONSULTATION TYPE — FK catre Speciality
            // ----------------------------------------------------------------
            modelBuilder.Entity<ConsultationType>()
                .HasOne(c => c.Speciality)
                .WithMany(s => s.ConsultationTypes)
                .HasForeignKey(c => c.SpecialityId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------------------------------------------
            // EQUIPMENT — FK catre Room
            // ----------------------------------------------------------------
            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.Room)
                .WithMany(r => r.Equipments)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}