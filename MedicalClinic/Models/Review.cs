namespace MedicalClinic.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public int Score { get; set; }
        public string Comment { get; set; }
        public bool IsAnonymous { get; set; }
        public bool IsApproved { get; set; }
    }
}
