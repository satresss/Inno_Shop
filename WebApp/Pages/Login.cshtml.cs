using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Services;

namespace WebApp.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApiService _apiService;

        public LoginModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public string? SuccessMessage { get; set; }

        public void OnGet(bool confirmEmail = false, int userId = 0, string token = null)
        {
            // Если уже авторизован - перенаправляем на главную
            if (_apiService.IsAuthenticated())
            {
                Response.Redirect("/");
                return;
            }

            // Обработка подтверждения email
            if (confirmEmail && userId > 0 && !string.IsNullOrEmpty(token))
            {
                ConfirmEmail(userId, token!);
            }
        }

        private async void ConfirmEmail(int userId, string token)
        {
            try
            {
                await _apiService.PostAsync<dynamic>("UsersApi", "/auth/confirm-email", new
                {
                    UserId = userId,
                    Token = token
                });

                SuccessMessage = "Email успешно подтвержден! Теперь вы можете войти.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка подтверждения email: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ErrorMessage = "Заполните все поля";
                return Page();
            }

            try
            {
                var loginData = new { email, password };
                var response = await _apiService.PostAsync<AuthResponseDto>(
                    "UsersApi", "/api/auth/login", loginData);

                if (response != null && !string.IsNullOrEmpty(response.AccessToken))
                {
                    // Сохраняем токены в сессии
                    _apiService.SaveTokens(response.AccessToken, response.RefreshToken);
                    return RedirectToPage("/Index");
                }
                else
                {
                    ErrorMessage = "Неверный email или пароль";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                // Проверяем, является ли ошибка связанной с неподтвержденным email
                if (ex.Message.Contains("Email not confirmed", StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = "Email не подтвержден. Проверьте свою почту и перейдите по ссылке для подтверждения.";
                }
                else
                {
                    ErrorMessage = "Ошибка при входе. Проверьте подключение к серверу.";
                }
                return Page();
            }
        }
    }
}

