using Microsoft.AspNetCore.Identity;

namespace MedicalClinic.Models
{
    public class Administrator
    {
        public int AdministratorId { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public string Name { get; set; }
    }
}
