namespace MedicalClinic.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public int? NurseId { get; set; }
        public Nurse Nurse { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }

        public int ConsultationTypeId { get; set; }
        public ConsultationType ConsultationType { get; set; }

        public int Duration { get; set; }
        public string Status { get; set; }
        public string CancellationReason { get; set; }
        public DateTime StartTime { get; set; }

        // RELAȚIE
        public ICollection<MedicalRecordEntry> MedicalRecords { get; set; }
    }
}
