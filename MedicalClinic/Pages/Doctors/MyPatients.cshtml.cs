using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MedicalClinic.Data;
using MedicalClinic.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalClinic.Pages.Doctors
{
    [Authorize(Roles = "Doctor")]
    public class MyPatientsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MyPatientsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<MedicalClinic.Models.Patient> Patients { get; set; }

        public async Task OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            if (doctor == null)
            {
                Patients = new List<MedicalClinic.Models.Patient>();
                return;
            }

            Patients = await _context.Patients
                .Include(p => p.Appointments)
                .Where(p => p.Appointments.Any(a => a.DoctorId == doctor.DoctorId))
                .ToListAsync();
        }
    }
}