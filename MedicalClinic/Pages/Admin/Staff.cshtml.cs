using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MedicalClinic.Data;
using Doctor = MedicalClinic.Models.Doctor;
using Nurse = MedicalClinic.Models.Nurse;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MedicalClinic.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class StaffModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StaffModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Doctor> Doctors { get; set; }
        public List<Nurse> Nurses { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Numele este obligatoriu")]
        public string NewName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email-ul este obligatoriu")]
        [EmailAddress]
        public string NewEmail { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Parola este obligatorie")]
        [MinLength(6)]
        public string NewPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Rolul este obligatoriu")]
        public string NewRole { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            Doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.DoctorSpecialities)
                    .ThenInclude(ds => ds.Speciality)
                .ToListAsync();

            Nurses = await _context.Nurses
                .Include(n => n.User)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            var user = new IdentityUser
            {
                UserName = NewEmail,
                Email = NewEmail,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, NewPassword);
            if (!result.Succeeded)
            {
                ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                await LoadDataAsync();
                return Page();
            }

            await _userManager.AddToRoleAsync(user, NewRole);

            if (NewRole == "Doctor")
            {
                _context.Doctors.Add(new Doctor
                {
                    UserId = user.Id,
                    Name = NewName,
                    Specialization = "",
                    AverageRating = 0
                });
            }
            else if (NewRole == "Nurse")
            {
                _context.Nurses.Add(new Nurse
                {
                    UserId = user.Id,
                    Name = NewName
                });
            }

            await _context.SaveChangesAsync();
            SuccessMessage = $"Contul pentru {NewName} a fost creat cu succes!";
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteStaffAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _userManager.DeleteAsync(user);

            return RedirectToPage();
        }
    }
}