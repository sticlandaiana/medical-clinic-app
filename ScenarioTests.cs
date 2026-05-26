using MedicalClinic.Models;
using MedicalClinic.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedicalClinic.Tests
{
    public class ScenarioTests
    {
        private static string NewUserId() => Guid.NewGuid().ToString();

        // TEST-19: triaj — actualizare date existente
        [Fact]
        public async Task Triage_ExistingEntry_UpdatesCorrectly()
        {
            using var ctx = TestDbContextFactory.Create();
            var spec = new Speciality { Name = "Medicina interna" }; ctx.Specialities.Add(spec);
            var ct = new ConsultationType { Name = "Consult general", Duration = 30, RequiresNurse = false, Speciality = spec }; ctx.ConsultationTypes.Add(ct);
            var doctor = new Doctor { Name = "Dr. Stan", Specialization = "Generalist", UserId = NewUserId() }; ctx.Doctors.Add(doctor);
            var patient = new Patient { Name = "Elena Marin", UserId = NewUserId() }; ctx.Patients.Add(patient);
            var room = new Room { Name = "Sala 3", Status = "Available" }; ctx.Rooms.Add(room);
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
                BloodPressure = "120/80", Weight = 60.0, Temperature = 36.5, Diagnoses = "", Notes = ""
            };
            ctx.MedicalRecordEntries.Add(entry);
            await ctx.SaveChangesAsync();

            entry.BloodPressure = "140/90";
            entry.Weight = 61.0;
            entry.Temperature = 37.2;
            await ctx.SaveChangesAsync();

            var updated = await ctx.MedicalRecordEntries.FindAsync(entry.MedicalRecordEntryId);
            Assert.Equal("140/90", updated!.BloodPressure);
            Assert.Equal(61.0, updated.Weight);
        }

        // TEST-20: sala indisponibila exclusa din cautare
        [Fact]
        public async Task Room_WithUnavailableStatus_IsExcludedFromSearch()
        {
            using var ctx = TestDbContextFactory.Create();
            ctx.Rooms.AddRange(
                new Room { Name = "Sala A", Status = "Available" },
                new Room { Name = "Sala B", Status = "Unavailable" }
            );
            await ctx.SaveChangesAsync();

            var available = await ctx.Rooms.Where(r => r.Status == "Available").ToListAsync();
            Assert.Single(available);
            Assert.Equal("Sala A", available[0].Name);
        }

        // TEST-21: filtru specialitate pe sala
        [Fact]
        public async Task Room_SpecialityFilter_ReturnsCorrectRoom()
        {
            using var ctx = TestDbContextFactory.Create();
            var specCardio = new Speciality { Name = "Cardiologie" };
            var specDerma = new Speciality { Name = "Dermatologie" };
            ctx.Specialities.AddRange(specCardio, specDerma);
            await ctx.SaveChangesAsync();

            ctx.Rooms.AddRange(
                new Room { Name = "Sala Cardio", Status = "Available", SpecialityId = specCardio.SpecialityId },
                new Room { Name = "Sala Derma", Status = "Available", SpecialityId = specDerma.SpecialityId },
                new Room { Name = "Sala Generala", Status = "Available", SpecialityId = null }
            );
            await ctx.SaveChangesAsync();

            var compatible = await ctx.Rooms
                .Where(r => r.Status == "Available" &&
                            (r.SpecialityId == null || r.SpecialityId == specCardio.SpecialityId))
                .ToListAsync();

            Assert.Equal(2, compatible.Count);
        }

        // TEST-22: asignare asistenta-doctor
        [Fact]
        public async Task NurseDoctorAssignment_Created_CanBeQueried()
        {
            using var ctx = TestDbContextFactory.Create();
            var nurse = new Nurse { Name = "Asist. Bogdan", UserId = NewUserId() }; ctx.Nurses.Add(nurse);
            var doctor = new Doctor { Name = "Dr. Luca", Specialization = "Generalist", UserId = NewUserId() }; ctx.Doctors.Add(doctor);
            await ctx.SaveChangesAsync();

            ctx.NurseDoctorAssignments.Add(new NurseDoctorAssignment { NurseId = nurse.NurseId, DoctorId = doctor.DoctorId });
            await ctx.SaveChangesAsync();

            var assignment = await ctx.NurseDoctorAssignments
                .FirstOrDefaultAsync(a => a.NurseId == nurse.NurseId && a.DoctorId == doctor.DoctorId);

            Assert.NotNull(assignment);
        }

        // TEST-23: rating mediu doctor
        [Fact]
        public async Task Doctor_AverageRating_UpdatedCorrectly()
        {
            using var ctx = TestDbContextFactory.Create();
            var doctor = new Doctor { Name = "Dr. Mihai", Specialization = "Generalist", AverageRating = 0, UserId = NewUserId() }; ctx.Doctors.Add(doctor);
            var patient = new Patient { Name = "Client Test", UserId = NewUserId() }; ctx.Patients.Add(patient);
            await ctx.SaveChangesAsync();

            ctx.Reviews.AddRange(
                new Review { DoctorId = doctor.DoctorId, PatientId = patient.PatientId, Score = 4, IsApproved = true, Comment = "Bun" },
                new Review { DoctorId = doctor.DoctorId, PatientId = patient.PatientId, Score = 5, IsApproved = true, Comment = "Foarte bun" },
                new Review { DoctorId = doctor.DoctorId, PatientId = patient.PatientId, Score = 3, IsApproved = true, Comment = "Ok" }
            );
            await ctx.SaveChangesAsync();

            var avg = await ctx.Reviews
                .Where(r => r.DoctorId == doctor.DoctorId && r.IsApproved)
                .AverageAsync(r => (double)r.Score);

            doctor.AverageRating = avg;
            await ctx.SaveChangesAsync();

            var updatedDoctor = await ctx.Doctors.FindAsync(doctor.DoctorId);
            Assert.Equal(4.0, updatedDoctor!.AverageRating);
        }

        // TEST-24: asistenta libera asignata la programare
        [Fact]
        public async Task Nurse_Available_AssignedToAppointmentWithNurseRequired()
        {
            using var ctx = TestDbContextFactory.Create();
            var nurse = new Nurse { Name = "Asist. Alina", UserId = NewUserId() }; ctx.Nurses.Add(nurse);
            await ctx.SaveChangesAsync();

            var selectedStart = new DateTime(2026, 8, 1, 9, 0, 0);
            var selectedEnd = selectedStart.AddMinutes(45);

            var availableNurse = await ctx.Nurses
                .Where(n => !ctx.Appointments
                    .Any(a => a.NurseId == n.NurseId &&
                              a.StartTime < selectedEnd &&
                              a.StartTime.AddMinutes(a.Duration) > selectedStart &&
                              a.Status != "Cancelled"))
                .FirstOrDefaultAsync();

            Assert.NotNull(availableNurse);
            Assert.Equal(nurse.NurseId, availableNurse!.NurseId);
        }

        // TEST-25: alergie critica — flag IsCritical
        [Fact]
        public async Task Patient_CriticalAllergy_MarkedCorrectly()
        {
            using var ctx = TestDbContextFactory.Create();
            var patient = new Patient { Name = "Test Pacient", UserId = NewUserId() }; ctx.Patients.Add(patient);
            await ctx.SaveChangesAsync();

            ctx.Allergies.AddRange(
                new Allergy { PatientId = patient.PatientId, Description = "Aspirina", IsCritical = false },
                new Allergy { PatientId = patient.PatientId, Description = "Latex", IsCritical = true }
            );
            await ctx.SaveChangesAsync();

            var criticalCount = await ctx.Allergies
                .CountAsync(a => a.PatientId == patient.PatientId && a.IsCritical);

            Assert.Equal(1, criticalCount);
        }
    }
}
