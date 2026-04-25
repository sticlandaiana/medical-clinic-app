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
    public class EditRecordModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EditRecordModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public string PatientName { get; set; }
        public string AppointmentDate { get; set; }
        public int PatientId { get; set; }
        public string ErrorMessage { get; set; }

        [BindProperty]
        public int RecordId { get; set; }

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

        public async Task<IActionResult> OnGetAsync(int recordId)
        {
            var record = await _context.MedicalRecordEntries
                .Include(r => r.Patient)
                .Include(r => r.Appointment)
                .FirstOrDefaultAsync(r => r.MedicalRecordEntryId == recordId);

            if (record == null)
                return RedirectToPage("/Doctors/MyPatients");

            // Verifică că doctorul curent e cel care a creat fișa
            var currentUser = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            if (record.DoctorId != doctor.DoctorId)
            {
                ErrorMessage = "Nu ai permisiunea să editezi această fișă.";
                return RedirectToPage("/Doctors/MyPatients");
            }

            RecordId = recordId;
            PatientId = record.PatientId;
            PatientName = record.Patient.Name;
            AppointmentDate = record.Appointment.StartTime.ToString("dd/MM/yyyy HH:mm");
            BloodPressure = record.BloodPressure;
            Weight = record.Weight;
            Temperature = record.Temperature;
            Diagnoses = record.Diagnoses;
            Notes = record.Notes;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var record = await _context.MedicalRecordEntries
                .Include(r => r.Patient)
                .Include(r => r.Appointment)
                .FirstOrDefaultAsync(r => r.MedicalRecordEntryId == RecordId);

            if (record == null)
                return RedirectToPage("/Doctors/MyPatients");

            record.BloodPressure = BloodPressure ?? "";
            record.Weight = Weight;
            record.Temperature = Temperature;
            record.Diagnoses = Diagnoses;
            record.Notes = Notes ?? "";

            await _context.SaveChangesAsync();

            return RedirectToPage("/Doctors/PatientRecord", new { patientId = record.PatientId });
        }
    }
}
