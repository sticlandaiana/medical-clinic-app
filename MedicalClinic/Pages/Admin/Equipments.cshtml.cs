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
    public class EquipmentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EquipmentsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Equipment> Equipments { get; set; }
        public List<Room> Rooms { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Numele este obligatoriu")]
        public string NewEquipmentName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Sala este obligatorie")]
        public int? NewEquipmentRoomId { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            Equipments = await _context.Equipments
                .Include(e => e.Room)
                .ToListAsync();
            Rooms = await _context.Rooms
                .Where(r => r.Status == "Available")
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            _context.Equipments.Add(new Equipment
            {
                Name = NewEquipmentName,
                RoomId = NewEquipmentRoomId!.Value,
                Status = "Functional"
            });
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangeStatusAsync(int id, string status)
        {
            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment != null)
            {
                equipment.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment != null)
            {
                _context.Equipments.Remove(equipment);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}