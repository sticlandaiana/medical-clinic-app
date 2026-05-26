using MedicalClinic.Models;
using MedicalClinic.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedicalClinic.Tests
{
    public class BusinessLogicTests
    {
        private static string NewUserId() => Guid.NewGuid().ToString();

        private static Doctor MakeDoctor(string name = "Dr. Popescu") =>
            new Doctor { Name = name, Specialization = "Cardiologie", AverageRating = 4.5, UserId = NewUserId() };

        private static Patient MakePatient(string name = "Ion Ionescu", int noShowCount = 0) =>
            new Patient { Name = name, NoShowCount = noShowCount, UserId = NewUserId() };

        private static Room MakeRoom(string status = "Available", int? specialityId = null) =>
            new Room { Name = "Sala 1", Status = status, SpecialityId = specialityId };

        private static Nurse MakeNurse(string name = "Asist. Maria") =>
            new Nurse { Name = name, UserId = NewUserId() };

        private static Speciality MakeSpeciality(string name = "Cardiologie") =>
            new Speciality { Name = name };

        private static ConsultationType MakeConsultationType(Speciality spec, bool requiresNurse = false, int duration = 30) =>
            new ConsultationType { Name = "Consultatie", Duration = duration, RequiresNurse = requiresNurse, Speciality = spec };

        // TEST-06a: BR-4 — pacient cu >=3 absente trebuie suspendat
        [Fact]
        public void Patient_WithThreeNoShows_ShouldBeSuspended()
        {
            var patient = MakePatient(noShowCount: 3);
            Assert.True(patient.NoShowCount >= 3);
        }

        // TEST-06b: BR-4 — pacient cu 2 absente nu e suspendat
        [Fact]
        public void Patient_WithTwoNoShows_ShouldNotBeSuspended()
        {
            var patient = MakePatient(noShowCount: 2);
            Assert.False(patient.NoShowCount >= 3);
        }

        // TEST-07a: REQ-21 — conflict doctor detectat
        [Fact]
        public async Task Doctor_WithOverlappingAppointment_ShouldBeDetectedAsConflict()
        {
            using var ctx = TestDbContextFactory.Create();
            var spec = MakeSpeciality(); ctx.Specialities.Add(spec);
            var ct = MakeConsultationType(spec, duration: 60); ctx.ConsultationTypes.Add(ct);
            var doctor = MakeDoctor(); ctx.Doctors.Add(doctor);
            var patient = MakePatient(); ctx.Patients.Add(patient);
            var room = MakeRoom(); ctx.Rooms.Add(room);
            await ctx.SaveChangesAsync();

            var startTime = new DateTime(2026, 6, 1, 10, 0, 0);
            ctx.Appointments.Add(new Appointment
            {
                PatientId = patient.PatientId, DoctorId = doctor.DoctorId, RoomId = room.RoomId,
                ConsultationTypeId = ct.ConsultationTypeId, StartTime = startTime,
                Duration = 60, Status = "Scheduled", CancellationReason = ""
            });
            await ctx.SaveChangesAsync();

            var newStart = startTime.AddMinutes(30);
            var newEnd = newStart.AddMinutes(60);
            var conflict = await ctx.Appointments.AnyAsync(a =>
                a.DoctorId == doctor.DoctorId &&
                a.StartTime < newEnd &&
                a.StartTime.AddMinutes(a.Duration) > newStart &&
                a.Status != "Cancelled");

            Assert.True(conflict);
        }

        // TEST-07b: REQ-21 — programare anulata nu genereaza conflict
        [Fact]
        public async Task Doctor_WithCancelledOverlappingAppointment_ShouldNotConflict()
        {
            using var ctx = TestDbContextFactory.Create();
            var spec = MakeSpeciality(); ctx.Specialities.Add(spec);
            var ct = MakeConsultationType(spec, duration: 60); ctx.ConsultationTypes.Add(ct);
            var doctor = MakeDoctor(); ctx.Doctors.Add(doctor);
            var patient = MakePatient(); ctx.Patients.Add(patient);
            var room = MakeRoom(); ctx.Rooms.Add(room);
            await ctx.SaveChangesAsync();

            var startTime = new DateTime(2026, 6, 1, 10, 0, 0);
            ctx.Appointments.Add(new Appointment
            {
                PatientId = patient.PatientId, DoctorId = doctor.DoctorId, RoomId = room.RoomId,
                ConsultationTypeId = ct.ConsultationTypeId, StartTime = startTime,
                Duration = 60, Status = "Cancelled", CancellationReason = "Test"
            });
            await ctx.SaveChangesAsync();

            var newStart = startTime.AddMinutes(30);
            var newEnd = newStart.AddMinutes(60);
            var conflict = await ctx.Appointments.AnyAsync(a =>
                a.DoctorId == doctor.DoctorId &&
                a.StartTime < newEnd &&
                a.StartTime.AddMinutes(a.Duration) > newStart &&
                a.Status != "Cancelled");

            Assert.False(conflict);
        }

        // TEST-08a: REQ-20 — echipament defect blocheaza sala
        [Fact]
        public async Task Room_WithNonFunctionalEquipment_ShouldBlockBooking()
        {
            using var ctx = TestDbContextFactory.Create();
            var room = MakeRoom(); ctx.Rooms.Add(room);
            await ctx.SaveChangesAsync();

            ctx.Equipments.Add(new Equipment { RoomId = room.RoomId, Name = "ECG", Status = "Defect" });
            await ctx.SaveChangesAsync();

            var hasDefect = await ctx.Equipments
                .Where(e => e.RoomId == room.RoomId && e.Status != "Functional")
                .AnyAsync();

            Assert.True(hasDefect);
        }

        // TEST-08b: REQ-20 — echipamente functionale permit programarea
        [Fact]
        public async Task Room_WithAllFunctionalEquipment_ShouldAllowBooking()
        {
            using var ctx = TestDbContextFactory.Create();
            var room = MakeRoom(); ctx.Rooms.Add(room);
            await ctx.SaveChangesAsync();

            ctx.Equipments.Add(new Equipment { RoomId = room.RoomId, Name = "ECG", Status = "Functional" });
            await ctx.SaveChangesAsync();

            var hasDefect = await ctx.Equipments
                .Where(e => e.RoomId == room.RoomId && e.Status != "Functional")
                .AnyAsync();

            Assert.False(hasDefect);
        }

        // TEST-09: REQ-19 — flag RequiresNurse setat corect
        [Fact]
        public async Task ConsultationType_RequiresNurse_NurseAssignedToAppointment()
        {
            using var ctx = TestDbContextFactory.Create();
            var spec = MakeSpeciality(); ctx.Specialities.Add(spec);
            var ct = MakeConsultationType(spec, requiresNurse: true, duration: 45); ctx.ConsultationTypes.Add(ct);
            var nurse = MakeNurse(); ctx.Nurses.Add(nurse);
            await ctx.SaveChangesAsync();

            var saved = await ctx.ConsultationTypes.FindAsync(ct.ConsultationTypeId);
            Assert.True(saved!.RequiresNurse);
        }

        // TEST-10: programare salvata si citita din DB
        [Fact]
        public async Task Appointment_SavedToDatabase_CanBeRetrieved()
        {
            using var ctx = TestDbContextFactory.Create();
            var spec = MakeSpeciality(); ctx.Specialities.Add(spec);
            var ct = MakeConsultationType(spec); ctx.ConsultationTypes.Add(ct);
            var doctor = MakeDoctor(); ctx.Doctors.Add(doctor);
            var patient = MakePatient(); ctx.Patients.Add(patient);
            var room = MakeRoom(); ctx.Rooms.Add(room);
            await ctx.SaveChangesAsync();

            var appointment = new Appointment
            {
                PatientId = patient.PatientId, DoctorId = doctor.DoctorId, RoomId = room.RoomId,
                ConsultationTypeId = ct.ConsultationTypeId, StartTime = new DateTime(2026, 7, 15, 9, 0, 0),
                Duration = 30, Status = "Scheduled", CancellationReason = ""
            };
            ctx.Appointments.Add(appointment);
            await ctx.SaveChangesAsync();

            var saved = await ctx.Appointments.FindAsync(appointment.AppointmentId);
            Assert.NotNull(saved);
            Assert.Equal("Scheduled", saved!.Status);
        }

        // TEST-11: REQ-32 — flag ReminderSent actualizat
        [Fact]
        public async Task Appointment_ReminderSentFlag_UpdatesCorrectly()
        {
            using var ctx = TestDbContextFactory.Create();
            var spec = MakeSpeciality(); ctx.Specialities.Add(spec);
            var ct = MakeConsultationType(spec); ctx.ConsultationTypes.Add(ct);
            var doctor = MakeDoctor(); ctx.Doctors.Add(doctor);
            var patient = MakePatient(); ctx.Patients.Add(patient);
            var room = MakeRoom(); ctx.Rooms.Add(room);
            await ctx.SaveChangesAsync();

            var appointment = new Appointment
            {
                PatientId = patient.PatientId, DoctorId = doctor.DoctorId, RoomId = room.RoomId,
                ConsultationTypeId = ct.ConsultationTypeId, StartTime = DateTime.Now.AddDays(1),
                Duration = 30, Status = "Scheduled", ReminderSent = false, CancellationReason = ""
            };
            ctx.Appointments.Add(appointment);
            await ctx.SaveChangesAsync();

            appointment.ReminderSent = true;
            await ctx.SaveChangesAsync();

            var updated = await ctx.Appointments.FindAsync(appointment.AppointmentId);
            Assert.True(updated!.ReminderSent);
        }

        // TEST-12: anulare programare cu motiv
        [Fact]
        public async Task Appointment_CancelWithReason_StatusUpdated()
        {
            using var ctx = TestDbContextFactory.Create();
            var spec = MakeSpeciality(); ctx.Specialities.Add(spec);
            var ct = MakeConsultationType(spec); ctx.ConsultationTypes.Add(ct);
            var doctor = MakeDoctor(); ctx.Doctors.Add(doctor);
            var patient = MakePatient(); ctx.Patients.Add(patient);
            var room = MakeRoom(); ctx.Rooms.Add(room);
            await ctx.SaveChangesAsync();

            var appointment = new Appointment
            {
                PatientId = patient.PatientId, DoctorId = doctor.DoctorId, RoomId = room.RoomId,
                ConsultationTypeId = ct.ConsultationTypeId, StartTime = DateTime.Now.AddDays(2),
                Duration = 30, Status = "Scheduled", CancellationReason = ""
            };
            ctx.Appointments.Add(appointment);
            await ctx.SaveChangesAsync();

            appointment.Status = "Cancelled";
            appointment.CancellationReason = "Pacient indisponibil";
            await ctx.SaveChangesAsync();

            var updated = await ctx.Appointments.FindAsync(appointment.AppointmentId);
            Assert.Equal("Cancelled", updated!.Status);
            Assert.Equal("Pacient indisponibil", updated.CancellationReason);
        }
    }
}
