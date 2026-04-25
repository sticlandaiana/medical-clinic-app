using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using MedicalClinic.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalClinic.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class PatientsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PatientsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<MedicalClinic.Models.Patient> Patients { get; set; }
        public string SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            Patients = await _context.Patients
                .Include(p => p.User)
                .OrderByDescending(p => p.NoShowCount)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostResetNoShowAsync(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient != null)
            {
                patient.NoShowCount = 0;
                await _context.SaveChangesAsync();
                SuccessMessage = $"No-show resetat pentru {patient.Name}.";
            }

            await LoadDataAsync();
            return Page();
        }
    }
}