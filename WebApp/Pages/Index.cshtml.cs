using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Services;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ApiService _apiService;

    public IndexModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public bool IsAuthenticated { get; set; }

    public void OnGet()
    {
        IsAuthenticated = _apiService.IsAuthenticated();
    }
}
