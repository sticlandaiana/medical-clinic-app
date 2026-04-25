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

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            if (doctor == null)
            {
                Appointments = new List<Appointment>();
                return;
            }

            Appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Room)
                .Include(a => a.Nurse)
                .Include(a => a.ConsultationType)
                .Where(a => a.DoctorId == doctor.DoctorId)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment != null)
            {
                appointment.Status = "Completed";
                await _context.SaveChangesAsync();
            }

            SuccessMessage = "Consultația a fost finalizată.";
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostNoShowAsync(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment != null)
            {
                appointment.Status = "NoShow";

                // BR-4: incrementează no-show counter
                var patient = appointment.Patient;
                patient.NoShowCount++;

                await _context.SaveChangesAsync();
            }

            SuccessMessage = "Pacientul a fost marcat ca absent.";
            await LoadDataAsync();
            return Page();
        }
    }
}