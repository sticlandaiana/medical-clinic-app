using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MedicalClinic.Data;
using MedicalClinic.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalClinic.Pages.Patients
{
    [Authorize(Roles = "Patient")]
    public class ReviewsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Appointment> CompletedAppointments { get; set; }
        public List<Review> MyReviews { get; set; }
        public HashSet<int> ReviewedAppointmentIds { get; set; }
        public string SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (patient == null)
            {
                CompletedAppointments = new List<Appointment>();
                MyReviews = new List<Review>();
                ReviewedAppointmentIds = new HashSet<int>();
                return;
            }

            CompletedAppointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.ConsultationType)
                .Where(a => a.PatientId == patient.PatientId && a.Status == "Completed")
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

            MyReviews = await _context.Reviews
                .Include(r => r.Doctor)
                .Where(r => r.PatientId == patient.PatientId)
                .ToListAsync();

            // Doctori deja recenzați de pacient
            var reviewedDoctorIds = MyReviews.Select(r => r.DoctorId).ToHashSet();
            ReviewedAppointmentIds = CompletedAppointments
                .Where(a => reviewedDoctorIds.Contains(a.DoctorId))
                .Select(a => a.AppointmentId)
                .ToHashSet();
        }

        public async Task<IActionResult> OnPostAsync(int doctorId, int score,
            string comment, bool isAnonymous)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            // Verifică dacă a mai dat review acestui doctor
            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.PatientId == patient.PatientId && r.DoctorId == doctorId);

            if (!alreadyReviewed)
            {
                _context.Reviews.Add(new Review
                {
                    PatientId = patient.PatientId,
                    DoctorId = doctorId,
                    Score = score,
                    Comment = comment ?? "",
                    IsAnonymous = isAnonymous,
                    IsApproved = false
                });

                // Actualizează rating-ul mediu al doctorului
                await _context.SaveChangesAsync();
                var allReviews = await _context.Reviews
                    .Where(r => r.DoctorId == doctorId && r.IsApproved)
                    .ToListAsync();

                var doctor = await _context.Doctors.FindAsync(doctorId);
                if (allReviews.Any())
                    doctor.AverageRating = allReviews.Average(r => r.Score);

                await _context.SaveChangesAsync();
            }

            SuccessMessage = "Recenzia a fost trimisă și așteaptă aprobare!";
            await LoadDataAsync();
            return Page();
        }
    }
}
