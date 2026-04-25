using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using MedicalClinic.Data;
using MedicalClinic.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalClinic.Pages.Nurses
{
    [Authorize(Roles = "Nurse")]
    public class DailyReportModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DailyReportModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Appointment> Appointments { get; set; }

        public async Task OnGetAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            Appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Room)
                .Include(a => a.Nurse)
                .Include(a => a.ConsultationType)
                .Where(a => a.StartTime >= today && a.StartTime < tomorrow)
                .OrderBy(a => a.StartTime)
                .ToListAsync();
        }
    }
}