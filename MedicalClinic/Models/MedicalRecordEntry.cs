namespace MedicalClinic.Models
{
    public class MedicalRecordEntry
    {
        public int MedicalRecordEntryId { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public string Notes { get; set; }
        public string BloodPressure { get; set; }
        public double Weight { get; set; }
        public double Temperature { get; set; }
        public string Diagnoses { get; set; }
    }
}
