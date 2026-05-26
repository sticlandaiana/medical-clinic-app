using MedicalClinic.Models;
using Xunit;

namespace MedicalClinic.Tests
{
    /// <summary>
    /// TEST-01 … TEST-05 — Validare proprietăți modele de domeniu
    /// </summary>
    public class ModelTests
    {
        // TEST-01: Appointment – valorile implicite sunt corecte
        [Fact]
        public void Appointment_DefaultReminderSent_IsFalse()
        {
            var appointment = new Appointment();
            Assert.False(appointment.ReminderSent);
        }

        // TEST-02: Appointment – statusurile posibile sunt recunoscute
        [Theory]
        [InlineData("Scheduled")]
        [InlineData("Completed")]
        [InlineData("Cancelled")]
        [InlineData("NoShow")]
        [InlineData("NeedsRescheduling")]
        public void Appointment_KnownStatuses_AreValidStrings(string status)
        {
            var appointment = new Appointment { Status = status };
            Assert.False(string.IsNullOrEmpty(appointment.Status));
            Assert.Equal(status, appointment.Status);
        }

        // TEST-03: Patient – NoShowCount pornește de la zero implicit
        [Fact]
        public void Patient_DefaultNoShowCount_IsZero()
        {
            var patient = new Patient();
            Assert.Equal(0, patient.NoShowCount);
        }

        // TEST-04: Review – scorul se poate seta și se păstrează
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void Review_Score_IsStoredCorrectly(int score)
        {
            var review = new Review { Score = score };
            Assert.Equal(score, review.Score);
        }

        // TEST-05: MedicalRecordEntry – câmpurile numerice se salvează corect
        [Fact]
        public void MedicalRecordEntry_Fields_AreStoredCorrectly()
        {
            var entry = new MedicalRecordEntry
            {
                BloodPressure = "120/80",
                Weight = 75.5,
                Temperature = 36.8,
                Diagnoses = "Gripal",
                Notes = "Repaus 3 zile"
            };

            Assert.Equal("120/80", entry.BloodPressure);
            Assert.Equal(75.5, entry.Weight);
            Assert.Equal(36.8, entry.Temperature);
        }
    }
}
