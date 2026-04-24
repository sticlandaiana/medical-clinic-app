namespace MedicalClinic.Models
{
    public class ConsultationType
    {
        public int ConsultationTypeId { get; set; }

        public int SpecialityId { get; set; }
        public Speciality Speciality { get; set; }

        public string Name { get; set; }
        public int Duration { get; set; }
        public bool RequiresNurse { get; set; }

        // RELAȚII
        public ICollection<Appointment> Appointments { get; set; }
    }
}
