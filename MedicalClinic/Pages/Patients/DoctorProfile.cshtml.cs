using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using MedicalClinic.Data;
using MedicalClinic.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalClinic.Pages.Patients
{
    [Authorize(Roles = "Patient")]
    public class DoctorProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DoctorProfileModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string DoctorName { get; set; }
        public string Specialities { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<Review> Reviews { get; set; }

        public async Task<IActionResult> OnGetAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.DoctorSpecialities)
                    .ThenInclude(ds => ds.Speciality)
                .Include(d => d.Reviews)
                    .ThenInclude(r => r.Patient)
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            if (doctor == null)
                return RedirectToPage("/Patients/Dashboard");

            DoctorName = doctor.Name;
            Specialities = string.Join(", ",
                doctor.DoctorSpecialities.Select(ds => ds.Speciality.Name));
            AverageRating = doctor.AverageRating;

            Reviews = doctor.Reviews
                .Where(r => r.IsApproved)
                .ToList();

            TotalReviews = Reviews.Count;

            return Page();
        }
    }
}
