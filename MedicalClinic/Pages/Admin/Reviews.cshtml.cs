using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using MedicalClinic.Data;
using MedicalClinic.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalClinic.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class ReviewsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReviewsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Review> Reviews { get; set; }

        public async Task OnGetAsync()
        {
            Reviews = await _context.Reviews
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .OrderBy(r => r.IsApproved)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                review.IsApproved = true;
                await _context.SaveChangesAsync();

                // Recalculează rating-ul doctorului
                var allReviews = await _context.Reviews
                    .Where(r => r.DoctorId == review.DoctorId && r.IsApproved)
                    .ToListAsync();

                var doctor = await _context.Doctors.FindAsync(review.DoctorId);
                if (allReviews.Any())
                    doctor.AverageRating = allReviews.Average(r => r.Score);

                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}