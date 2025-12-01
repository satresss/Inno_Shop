using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Services;

namespace WebApp.Pages
{
    public class CreateProductModel : PageModel
    {
        private readonly ApiService _apiService;

        public CreateProductModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            if (!_apiService.IsAuthenticated())
            {
                Response.Redirect("/Login");
            }
        }

        public async Task<IActionResult> OnPostAsync(string name, string description, decimal price, bool isAvailable = true)
        {
            if (!_apiService.IsAuthenticated())
            {
                return RedirectToPage("/Login");
            }

            if (string.IsNullOrEmpty(name))
            {
                ErrorMessage = "Название обязательно";
                return Page();
            }

            if (price <= 0)
            {
                ErrorMessage = "Цена должна быть больше 0";
                return Page();
            }

            try
            {
                var productData = new
                {
                    name,
                    description,
                    price,
                    isAvailable
                };

                var result = await _apiService.PostAsync<ProductDto>("ProductsApi", "/products", productData);

                if (result != null)
                {
                    SuccessMessage = "Продукт создан успешно!";
                    return RedirectToPage("/Products");
                }
                else
                {
                    ErrorMessage = "Ошибка при создании продукта";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                return Page();
            }
        }
    }
}
