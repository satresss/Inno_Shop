using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Services;

namespace WebApp.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly ApiService _apiService;

        public LogoutModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult OnGet()
        {
            _apiService.ClearTokens();
            return RedirectToPage("/Index");
        }
    }
}

