using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using MedicalClinic.Data;
using MedicalClinic.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MedicalClinic.Pages.Doctors
{
    [Authorize(Roles = "Doctor")]
    public class ManageAllergiesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ManageAllergiesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string PatientName { get; set; }
        public int PatientId { get; set; }
        public List<Allergy> Allergies { get; set; }
        public string SuccessMessage { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Descrierea este obligatorie")]
        public string Description { get; set; }

        [BindProperty]
        public bool IsCritical { get; set; }

        public async Task<IActionResult> OnGetAsync(int patientId)
        {
            await LoadDataAsync(patientId);
            return Page();
        }

        private async Task LoadDataAsync(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null) return;

            PatientId = patientId;
            PatientName = patient.Name;

            Allergies = await _context.Allergies
                .Where(a => a.PatientId == patientId)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(int patientId)
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync(patientId);
                return Page();
            }

            _context.Allergies.Add(new Allergy
            {
                PatientId = patientId,
                Description = Description,
                IsCritical = IsCritical
            });

            await _context.SaveChangesAsync();
            SuccessMessage = "Alergia a fost adăugată.";
            await LoadDataAsync(patientId);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int allergyId, int patientId)
        {
            var allergy = await _context.Allergies.FindAsync(allergyId);
            if (allergy != null)
            {
                _context.Allergies.Remove(allergy);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { patientId });
        }
    }
}