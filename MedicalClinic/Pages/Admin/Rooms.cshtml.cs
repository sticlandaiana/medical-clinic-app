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
    public class RoomsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RoomsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Room> Rooms { get; set; }
        public List<Speciality> Specialities { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Numele este obligatoriu")]
        public string NewRoomName { get; set; }

        [BindProperty]
        public int? NewRoomSpecialityId { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            Rooms = await _context.Rooms
                .Include(r => r.Speciality)
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

            _context.Rooms.Add(new Room
            {
                Name = NewRoomName,
                SpecialityId = NewRoomSpecialityId,
                Status = "Available"
            });
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                room.Status = room.Status == "Available" ? "Unavailable" : "Available";
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}