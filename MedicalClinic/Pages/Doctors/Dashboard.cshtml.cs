using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace MedicalClinic.Pages.Doctors
{
    [Authorize(Roles = "Doctor")]
    public class DashboardModel : PageModel
    {
        public void OnGet() { }
    }
}
