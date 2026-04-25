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
    public class SpecialitiesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SpecialitiesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Speciality> Specialities { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Numele este obligatoriu")]
        public string NewSpecialityName { get; set; }

        public async Task OnGetAsync()
        {
            Specialities = await _context.Specialities.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Specialities = await _context.Specialities.ToListAsync();
                return Page();
            }

            _context.Specialities.Add(new Speciality { Name = NewSpecialityName });
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var speciality = await _context.Specialities.FindAsync(id);
            if (speciality != null)
            {
                _context.Specialities.Remove(speciality);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
