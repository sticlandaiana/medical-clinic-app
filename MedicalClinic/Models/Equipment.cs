namespace MedicalClinic.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }

        public string Name { get; set; }
        public string Status { get; set; }
    }
}
