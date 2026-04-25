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
    public class ConsultationTypesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ConsultationTypesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ConsultationType> ConsultationTypes { get; set; }
        public List<Speciality> Specialities { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Numele este obligatoriu")]
        public string NewName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Specialitatea este obligatorie")]
        public int? NewSpecialityId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Durata este obligatorie")]
        [Range(5, 480, ErrorMessage = "Durata trebuie să fie între 5 și 480 minute")]
        public int? NewDuration { get; set; }

        [BindProperty]
        public bool NewRequiresNurse { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            ConsultationTypes = await _context.ConsultationTypes
                .Include(c => c.Speciality)
                .ToListAsync();
            Specialities = await _context.Specialities.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            _context.ConsultationTypes.Add(new ConsultationType
            {
                Name = NewName,
                SpecialityId = NewSpecialityId!.Value,
                Duration = NewDuration!.Value,
                RequiresNurse = NewRequiresNurse
            });
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var ct = await _context.ConsultationTypes.FindAsync(id);
            if (ct != null)
            {
                _context.ConsultationTypes.Remove(ct);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}