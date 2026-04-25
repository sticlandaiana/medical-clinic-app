using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace MedicalClinic.Pages.Patients
{
    [Authorize(Roles = "Patient")]
    public class DashboardModel : PageModel
    {
        public void OnGet() { }
    }
}
