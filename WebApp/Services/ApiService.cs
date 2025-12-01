using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // Получает токен из сессии
        private string? GetAccessToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
        }

        // Настраивает HttpClient с токеном авторизации
        private HttpClient GetAuthenticatedClient(string apiBaseUrl)
        {
            _httpClient.BaseAddress = new Uri(apiBaseUrl);
            _httpClient.DefaultRequestHeaders.Clear();
            
            var token = GetAccessToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);
            }
            
            return _httpClient;
        }

        // GET запрос
        public async Task<T?> GetAsync<T>(string apiName, string endpoint)
        {
            try
            {
                var apiUrl = _configuration[$"ApiSettings:{apiName}"];
                if (string.IsNullOrEmpty(apiUrl))
                {
                    return default(T);
                }
                var client = GetAuthenticatedClient(apiUrl);
                var response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return default(T);
            }
        }

        // Поиск продуктов с фильтрами
        public async Task<List<ProductDto>?> SearchProductsAsync(string apiName, ProductFilterDto filter)
        {
            try
            {
                var apiUrl = _configuration[$"ApiSettings:{apiName}"];
                if (string.IsNullOrEmpty(apiUrl))
                {
                    return new List<ProductDto>();
                }

                var client = GetAuthenticatedClient(apiUrl);

                // Строим query string из параметров фильтра
                var queryParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(filter.SearchTerm)}");

                if (filter.MinPrice.HasValue)
                    queryParams.Add($"minPrice={filter.MinPrice.Value}");

                if (filter.MaxPrice.HasValue)
                    queryParams.Add($"maxPrice={filter.MaxPrice.Value}");

                if (filter.IsAvailable.HasValue)
                    queryParams.Add($"isAvailable={filter.IsAvailable.Value.ToString().ToLower()}");

                if (filter.CreatedByUserId.HasValue)
                    queryParams.Add($"createdByUserId={filter.CreatedByUserId.Value}");

                queryParams.Add($"pageNumber={filter.PageNumber}");
                queryParams.Add($"pageSize={filter.PageSize}");

                var queryString = string.Join("&", queryParams);
                var endpoint = $"/products/search?{queryString}";

                Console.WriteLine($"[ApiService] Поиск продуктов: {apiUrl}{endpoint}");

                var response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ProductDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[ApiService] HttpRequestException при поиске продуктов: {ex.Message}");
                return new List<ProductDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService] Exception при поиске продуктов: {ex.Message}");
                return new List<ProductDto>();
            }
        }

        // POST запрос
        public async Task<T?> PostAsync<T>(string apiName, string endpoint, object? data = null)
        {
            try
            {
                var apiUrl = _configuration[$"ApiSettings:{apiName}"];
                if (string.IsNullOrEmpty(apiUrl))
                {
                    Console.WriteLine($"[ApiService] API URL для {apiName} не найден в конфигурации");
                    return default(T);
                }

                Console.WriteLine($"[ApiService] Отправка POST запроса: {apiUrl}{endpoint}");
                var client = GetAuthenticatedClient(apiUrl);

                HttpContent? content = null;
                if (data != null)
                {
                    var json = JsonSerializer.Serialize(data);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                    Console.WriteLine($"[ApiService] Тело запроса: {json}");
                }

                var response = await client.PostAsync(endpoint, content);
                Console.WriteLine($"[ApiService] Статус ответа: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ApiService] Ошибка: {errorContent}");
                }

                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ApiService] Ответ получен: {responseJson.Substring(0, Math.Min(100, responseJson.Length))}...");
                return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[ApiService] HttpRequestException: {ex.Message}");
                Console.WriteLine($"[ApiService] StackTrace: {ex.StackTrace}");
                return default(T);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService] Exception: {ex.Message}");
                Console.WriteLine($"[ApiService] StackTrace: {ex.StackTrace}");
                return default(T);
            }
        }

        // Сохраняет токены в сессии
        public void SaveTokens(string accessToken, string refreshToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Session.SetString("AccessToken", accessToken);
                httpContext.Session.SetString("RefreshToken", refreshToken);
            }
        }

        // Проверяет, авторизован ли пользователь
        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(GetAccessToken());
        }

        // Очищает токены (выход)
        public void ClearTokens()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Session.Remove("AccessToken");
                httpContext.Session.Remove("RefreshToken");
            }
        }
    }

    // DTO для фильтрации продуктов
    public class ProductFilterDto
    {
        public string? SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsAvailable { get; set; }
        public int? CreatedByUserId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // DTO для продукта
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // DTO для ответа авторизации
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}

