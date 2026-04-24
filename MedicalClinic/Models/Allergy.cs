namespace MedicalClinic.Models
{
    public class Allergy
    {
        public int AllergyId { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public string Description { get; set; }
        public bool IsCritical { get; set; }
    }
}
