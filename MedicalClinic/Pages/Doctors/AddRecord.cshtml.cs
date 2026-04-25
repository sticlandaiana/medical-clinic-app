using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MedicalClinic.Data;
using MedicalClinic.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MedicalClinic.Pages.Doctors
{
    [Authorize(Roles = "Doctor")]
    public class AddRecordModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AddRecordModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public string PatientName { get; set; }
        public string AppointmentDate { get; set; }
        public string ErrorMessage { get; set; }

        [BindProperty]
        public int AppointmentId { get; set; }

        [BindProperty]
        public string BloodPressure { get; set; }

        [BindProperty]
        public double Weight { get; set; }

        [BindProperty]
        public double Temperature { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Diagnosticul este obligatoriu")]
        public string Diagnoses { get; set; }

        [BindProperty]
        public string Notes { get; set; }

        public async Task<IActionResult> OnGetAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return RedirectToPage("/Doctors/MyAppointments");

            AppointmentId = appointmentId;
            PatientName = appointment.Patient.Name;
            AppointmentDate = appointment.StartTime.ToString("dd/MM/yyyy HH:mm");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var appt = await _context.Appointments
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.AppointmentId == AppointmentId);
                PatientName = appt?.Patient.Name;
                AppointmentDate = appt?.StartTime.ToString("dd/MM/yyyy HH:mm");
                return Page();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == AppointmentId);

            // Verifică dacă există deja o fișă
            var exists = await _context.MedicalRecordEntries
                .AnyAsync(m => m.AppointmentId == AppointmentId);

            if (exists)
            {
                ErrorMessage = "Există deja o fișă pentru această consultație.";
                return Page();
            }

            _context.MedicalRecordEntries.Add(new MedicalRecordEntry
            {
                DoctorId = doctor.DoctorId,
                PatientId = appointment.PatientId,
                AppointmentId = AppointmentId,
                BloodPressure = BloodPressure ?? "",
                Weight = Weight,
                Temperature = Temperature,
                Diagnoses = Diagnoses,
                Notes = Notes ?? ""
            });

            await _context.SaveChangesAsync();
            return RedirectToPage("/Doctors/MyAppointments");
        }
    }
}