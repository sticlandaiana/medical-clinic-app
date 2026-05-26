using MedicalClinic.Models;
using MedicalClinic.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedicalClinic.Tests
{
    public class CrudTests
    {
        private static string NewUserId() => Guid.NewGuid().ToString();

        // TEST-13: alergie — adaugare si stergere
        [Fact]
        public async Task Allergy_AddAndDelete_WorksCorrectly()
        {
            using var ctx = TestDbContextFactory.Create();
            var patient = new Patient { Name = "Mihai Pop", UserId = NewUserId() };
            ctx.Patients.Add(patient);
            await ctx.SaveChangesAsync();

            var allergy = new Allergy { PatientId = patient.PatientId, Description = "Penicilina", IsCritical = true };
            ctx.Allergies.Add(allergy);
            await ctx.SaveChangesAsync();

            Assert.NotEqual(0, allergy.AllergyId);

            ctx.Allergies.Remove(allergy);
            await ctx.SaveChangesAsync();

            var deleted = await ctx.Allergies.FindAsync(allergy.AllergyId);
            Assert.Null(deleted);
        }

        // TEST-14: fisa medicala — adaugare si interogare
        [Fact]
        public async Task MedicalRecordEntry_Added_CanBeQueried()
        {
            using var ctx = TestDbContextFactory.Create();
            var spec = new Speciality { Name = "Neurologie" };
            ctx.Specialities.Add(spec);
            var ct = new ConsultationType { Name = "Consult", Duration = 30, RequiresNurse = false, Speciality = spec };
            ctx.ConsultationTypes.Add(ct);
            var doctor = new Doctor { Name = "Dr. Ion", Specialization = "Neurologie", UserId = NewUserId() };
            ctx.Doctors.Add(doctor);
            var patient = new Patient { Name = "Ana Dumitrescu", UserId = NewUserId() };
            ctx.Patients.Add(patient);
            var room = new Room { Name = "Sala 2", Status = "Available" };
            ctx.Rooms.Add(room);
            await ctx.SaveChangesAsync();

            var appt = new Appointment
            {
                PatientId = patient.PatientId, DoctorId = doctor.DoctorId, RoomId = room.RoomId,
                ConsultationTypeId = ct.ConsultationTypeId, StartTime = DateTime.Now,
                Duration = 30, Status = "Scheduled", CancellationReason = ""
            };
            ctx.Appointments.Add(appt);
            await ctx.SaveChangesAsync();

            var entry = new MedicalRecordEntry
            {
                DoctorId = doctor.DoctorId, PatientId = patient.PatientId, AppointmentId = appt.AppointmentId,
                BloodPressure = "130/85", Weight = 68.0, Temperature = 37.1,
                Diagnoses = "Migrena", Notes = "Tratament prescris"
            };
            ctx.MedicalRecordEntries.Add(entry);
            await ctx.SaveChangesAsync();

            var saved = await ctx.MedicalRecordEntries
                .FirstOrDefaultAsync(m => m.AppointmentId == appt.AppointmentId);

            Assert.NotNull(saved);
            Assert.Equal("Migrena", saved!.Diagnoses);
        }

        // TEST-15: specialitati — adaugare multipla si numarare
        [Fact]
        public async Task Speciality_AddMultiple_AllRetrieved()
        {
            using var ctx = TestDbContextFactory.Create();
            ctx.Specialities.AddRange(
                new Speciality { Name = "Cardiologie" },
                new Speciality { Name = "Dermatologie" },
                new Speciality { Name = "Ortopedie" }
            );
            await ctx.SaveChangesAsync();

            var count = await ctx.Specialities.CountAsync();
            Assert.Equal(3, count);
        }

        // TEST-16: asociere doctor-specialitate
        [Fact]
        public async Task DoctorSpeciality_Linked_CanBeQueried()
        {
            using var ctx = TestDbContextFactory.Create();
            var spec = new Speciality { Name = "Chirurgie" };
            ctx.Specialities.Add(spec);
            var doctor = new Doctor { Name = "Dr. Georgescu", Specialization = "Chirurgie", UserId = NewUserId() };
            ctx.Doctors.Add(doctor);
            await ctx.SaveChangesAsync();

            ctx.DoctorSpecialities.Add(new DoctorSpeciality { DoctorId = doctor.DoctorId, SpecialityId = spec.SpecialityId });
            await ctx.SaveChangesAsync();

            var hasSpec = await ctx.DoctorSpecialities
                .AnyAsync(ds => ds.DoctorId == doctor.DoctorId && ds.SpecialityId == spec.SpecialityId);

            Assert.True(hasSpec);
        }

        // TEST-17: review — aprobare si listare
        [Fact]
        public async Task Review_Approved_AppearsInApprovedList()
        {
            using var ctx = TestDbContextFactory.Create();
            var doctor = new Doctor { Name = "Dr. Radu", Specialization = "Generalist", UserId = NewUserId() };
            ctx.Doctors.Add(doctor);
            var patient = new Patient { Name = "Vasile Dinu", UserId = NewUserId() };
            ctx.Patients.Add(patient);
            await ctx.SaveChangesAsync();

            ctx.Reviews.AddRange(
                new Review { DoctorId = doctor.DoctorId, PatientId = patient.PatientId, Score = 5, IsApproved = true, Comment = "Excelent!" },
                new Review { DoctorId = doctor.DoctorId, PatientId = patient.PatientId, Score = 3, IsApproved = false, Comment = "Ok" }
            );
            await ctx.SaveChangesAsync();

            var approvedCount = await ctx.Reviews.CountAsync(r => r.IsApproved);
            Assert.Equal(1, approvedCount);
        }

        // TEST-18: audit log — salvare si interogare
        [Fact]
        public async Task AuditLog_Entry_SavedWithCorrectFields()
        {
            using var ctx = TestDbContextFactory.Create();
            var log = new AuditLog
            {
                UserId = "user-123", UserEmail = "admin@clinic.ro",
                Action = "DELETE", EntityType = "Appointment",
                Details = "Programare #42 stearsa", Timestamp = DateTime.Now
            };
            ctx.AuditLogs.Add(log);
            await ctx.SaveChangesAsync();

            var saved = await ctx.AuditLogs.FirstOrDefaultAsync(l => l.UserId == "user-123");
            Assert.NotNull(saved);
            Assert.Equal("DELETE", saved!.Action);
        }
    }
}
