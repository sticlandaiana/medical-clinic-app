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
    public class MyProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MyProfileModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public string DoctorName { get; set; }
        public string Specialities { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<Review> Reviews { get; set; }

        public async Task OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .Include(d => d.DoctorSpecialities)
                    .ThenInclude(ds => ds.Speciality)
                .Include(d => d.Reviews)
                    .ThenInclude(r => r.Patient)
                .FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            if (doctor == null) return;

            DoctorName = doctor.Name;
            Specialities = string.Join(", ",
                doctor.DoctorSpecialities.Select(ds => ds.Speciality.Name));
            AverageRating = doctor.AverageRating;

            Reviews = doctor.Reviews
                .Where(r => r.IsApproved)
                .ToList();

            TotalReviews = Reviews.Count;
        }
    }
}
