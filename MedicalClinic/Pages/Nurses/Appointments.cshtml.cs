using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MedicalClinic.Data;
using MedicalClinic.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MedicalClinic.Pages.Nurses
{
    [Authorize(Roles = "Nurse")]
    public class AppointmentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AppointmentsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Appointment> Appointments { get; set; }
        public List<MedicalClinic.Models.Patient> Patients { get; set; }
        public List<Speciality> Specialities { get; set; }
        public List<ConsultationType> ConsultationTypes { get; set; }
        public List<MedicalClinic.Models.Doctor> Doctors { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Pacientul este obligatoriu")]
        public int? SelectedPatientId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Specialitatea este obligatorie")]
        public int? SelectedSpecialityId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Tipul de consultație este obligatoriu")]
        public int? SelectedConsultationTypeId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Doctorul este obligatoriu")]
        public int? SelectedDoctorId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Data și ora sunt obligatorii")]
        public DateTime? SelectedDateTime { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            Appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.ConsultationType)
                .Include(a => a.Room)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

            Patients = await _context.Patients.ToListAsync();
            Specialities = await _context.Specialities.ToListAsync();
            ConsultationTypes = await _context.ConsultationTypes
                .Include(ct => ct.Speciality)
                .ToListAsync();
            Doctors = await _context.Doctors
                .Include(d => d.DoctorSpecialities)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            var consultationType = await _context.ConsultationTypes
                .FindAsync(SelectedConsultationTypeId);

            // Verifică conflict doctor
            var doctorConflict = await _context.Appointments
                .AnyAsync(a => a.DoctorId == SelectedDoctorId &&
                               a.StartTime < SelectedDateTime!.Value.AddMinutes(consultationType.Duration) &&
                               a.StartTime.AddMinutes(a.Duration) > SelectedDateTime.Value &&
                               a.Status != "Cancelled");

            if (doctorConflict)
            {
                ErrorMessage = "Doctorul nu este disponibil la ora selectată.";
                await LoadDataAsync();
                return Page();
            }

            // Găsește sală disponibilă
            var room = await _context.Rooms
                .Where(r => r.Status == "Available" &&
                       (r.SpecialityId == null || r.SpecialityId == SelectedSpecialityId))
                .FirstOrDefaultAsync();

            if (room == null)
            {
                ErrorMessage = "Nu există săli disponibile pentru această specialitate.";
                await LoadDataAsync();
                return Page();
            }

            // Verifică dacă necesită asistentă
            MedicalClinic.Models.Nurse nurse = null;
            if (consultationType.RequiresNurse)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                nurse = await _context.Nurses
                    .FirstOrDefaultAsync(n => n.UserId == currentUser.Id);
            }

            _context.Appointments.Add(new Appointment
            {
                PatientId = SelectedPatientId!.Value,
                DoctorId = SelectedDoctorId!.Value,
                RoomId = room.RoomId,
                ConsultationTypeId = SelectedConsultationTypeId!.Value,
                NurseId = nurse?.NurseId,
                StartTime = SelectedDateTime!.Value,
                Duration = consultationType.Duration,
                Status = "Scheduled",
                CancellationReason = ""
            });

            await _context.SaveChangesAsync();
            SuccessMessage = "Programarea a fost creată cu succes!";
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Cancelled";
                appointment.CancellationReason = "Anulat de asistentă";
                await _context.SaveChangesAsync();
            }

            SuccessMessage = "Programarea a fost anulată.";
            await LoadDataAsync();
            return Page();
        }
    }
}