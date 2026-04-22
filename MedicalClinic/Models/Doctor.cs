using Microsoft.AspNetCore.Identity;

namespace MedicalClinic.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public string Name { get; set; }
        public string Specialization { get; set; }
        public double AverageRating { get; set; }

        // RELAȚII
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<MedicalRecordEntry> MedicalRecords { get; set; }
        public ICollection<DoctorSpeciality> DoctorSpecialities { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
