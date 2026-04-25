using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MedicalClinic.Data;
using MedicalClinic.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalClinic.Pages.Patients
{
    [Authorize(Roles = "Patient")]
    public class MyAppointmentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MyAppointmentsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Appointment> Appointments { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (patient == null)
            {
                Appointments = new List<Appointment>();
                return;
            }

            Appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Room)
                .Include(a => a.ConsultationType)
                    .ThenInclude(ct => ct.Speciality)
                .Where(p => p.PatientId == patient.PatientId)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id &&
                                          a.PatientId == patient.PatientId);

            if (appointment == null)
            {
                ErrorMessage = "Programarea nu a fost găsită.";
                await LoadDataAsync();
                return Page();
            }

            // BR-3: anulare doar cu 24h înainte
            if ((appointment.StartTime - DateTime.Now).TotalHours < 24)
            {
                ErrorMessage = "Programările nu pot fi anulate cu mai puțin de 24 ore înainte. Contactați clinica telefonic.";
                await LoadDataAsync();
                return Page();
            }

            appointment.Status = "Cancelled";
            appointment.CancellationReason = "Anulat de pacient";
            await _context.SaveChangesAsync();

            SuccessMessage = "Programarea a fost anulată cu succes.";
            await LoadDataAsync();
            return Page();
        }
    }
}