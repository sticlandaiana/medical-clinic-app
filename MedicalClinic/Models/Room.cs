namespace MedicalClinic.Models
{
    public class Room
    {
        public int RoomId { get; set; }

        public string Name { get; set; }
        public string Status { get; set; }

        // RELAȚII
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}
