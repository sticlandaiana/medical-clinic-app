using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace MedicalClinic.Pages.Nurses
{
    [Authorize(Roles = "Nurse")]
    public class DashboardModel : PageModel
    {
        public void OnGet() { }
    }
}
