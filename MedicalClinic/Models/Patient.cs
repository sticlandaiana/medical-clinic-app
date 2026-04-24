using Microsoft.AspNetCore.Identity;

namespace MedicalClinic.Models
{
    public class Patient
    {
        public int PatientId { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public string Name { get; set; }
        public int NoShowCount { get; set; }

        // RELAȚII
        public ICollection<Allergy> Allergies { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<MedicalRecordEntry> MedicalRecords { get; set; }
        public ICollection<ExternalDocument> Documents { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
