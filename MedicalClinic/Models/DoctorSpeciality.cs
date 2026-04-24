namespace MedicalClinic.Models
{
    public class DoctorSpeciality
    {
        public int DoctorSpecialityId { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public int SpecialityId { get; set; }
        public Speciality Speciality { get; set; }
    }
}
