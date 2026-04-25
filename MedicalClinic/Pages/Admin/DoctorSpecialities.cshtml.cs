using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using MedicalClinic.Data;
using MedicalClinic.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MedicalClinic.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class DoctorSpecialitiesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DoctorSpecialitiesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<DoctorSpeciality> DoctorSpecialities { get; set; }
        public List<MedicalClinic.Models.Doctor> Doctors { get; set; }
        public List<Speciality> Specialities { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Doctorul este obligatoriu")]
        public int? SelectedDoctorId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Specialitatea este obligatorie")]
        public int? SelectedSpecialityId { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            DoctorSpecialities = await _context.DoctorSpecialities
                .Include(ds => ds.Doctor)
                .Include(ds => ds.Speciality)
                .ToListAsync();

            Doctors = await _context.Doctors.ToListAsync();
            Specialities = await _context.Specialities.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            // Verifică dacă asocierea există deja
            var exists = await _context.DoctorSpecialities
                .AnyAsync(ds => ds.DoctorId == SelectedDoctorId && ds.SpecialityId == SelectedSpecialityId);

            if (!exists)
            {
                _context.DoctorSpecialities.Add(new DoctorSpeciality
                {
                    DoctorId = SelectedDoctorId!.Value,
                    SpecialityId = SelectedSpecialityId!.Value
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var ds = await _context.DoctorSpecialities.FindAsync(id);
            if (ds != null)
            {
                _context.DoctorSpecialities.Remove(ds);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}