using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using MedicalClinic.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MedicalClinic.Pages.Nurses
{
    [Authorize(Roles = "Nurse")]
    public class EditAppointmentModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditAppointmentModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string ErrorMessage { get; set; }

        [BindProperty]
        public int AppointmentId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Data și ora sunt obligatorii")]
        public DateTime? NewDateTime { get; set; }

        public async Task<IActionResult> OnGetAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return RedirectToPage("/Nurses/Appointments");

            AppointmentId = appointmentId;
            PatientName = appointment.Patient.Name;
            DoctorName = appointment.Doctor.Name;
            NewDateTime = appointment.StartTime;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var appointment = await _context.Appointments
                .Include(a => a.ConsultationType)
                .FirstOrDefaultAsync(a => a.AppointmentId == AppointmentId);

            // Verifică conflict doctor la noua oră
            var doctorConflict = await _context.Appointments
                .AnyAsync(a => a.DoctorId == appointment.DoctorId &&
                               a.AppointmentId != AppointmentId &&
                               a.StartTime < NewDateTime!.Value.AddMinutes(appointment.Duration) &&
                               a.StartTime.AddMinutes(a.Duration) > NewDateTime.Value &&
                               a.Status != "Cancelled");

            if (doctorConflict)
            {
                ErrorMessage = "Doctorul nu este disponibil la ora selectată.";
                return Page();
            }

            appointment.StartTime = NewDateTime!.Value;
            await _context.SaveChangesAsync();

            return RedirectToPage("/Nurses/Appointments");
        }
    }
}