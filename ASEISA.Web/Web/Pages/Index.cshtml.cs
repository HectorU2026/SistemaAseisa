using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Seguridad/Login");
            }

            if(User.IsInRole("Superadministrador"))return RedirectToPage("/Plataforma/Index");
            return Page();
        }
    }
}
