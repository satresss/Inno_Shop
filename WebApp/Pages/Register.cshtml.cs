using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Services;

namespace WebApp.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ApiService _apiService;

        public RegisterModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            if (_apiService.IsAuthenticated())
            {
                Response.Redirect("/");
            }
        }

        public async Task<IActionResult> OnPostAsync(string name, string email, string password, string confirmPassword)
        {
            if (_apiService.IsAuthenticated())
            {
                return RedirectToPage("/Index");
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ErrorMessage = "Все поля обязательны для заполнения";
                return Page();
            }

            if (password != confirmPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                return Page();
            }

            if (password.Length < 6)
            {
                ErrorMessage = "Пароль должен содержать минимум 6 символов";
                return Page();
            }

            try
            {
                var registerData = new
                {
                    name,
                    email,
                    password,
                    confirmPassword
                };

                var result = await _apiService.PostAsync<dynamic>("UsersApi", "/auth/register", registerData);

                if (result != null)
                {
                    SuccessMessage = "Регистрация успешна! Проверьте email для подтверждения аккаунта.";
                    // Можно автоматически перенаправить на страницу входа через несколько секунд
                    return Page();
                }
                else
                {
                    ErrorMessage = "Ошибка при регистрации";
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
