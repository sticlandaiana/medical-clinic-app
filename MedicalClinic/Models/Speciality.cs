namespace MedicalClinic.Models
{
    public class Speciality
    {
        public int SpecialityId { get; set; }

        public string Name { get; set; }

        // RELAȚII
        public ICollection<DoctorSpeciality> DoctorSpecialities { get; set; }
        public ICollection<ConsultationType> ConsultationTypes { get; set; }
    }
}
