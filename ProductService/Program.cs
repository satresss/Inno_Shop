using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ProductService.Application.Interfaces;
using ProductService.Application.Services;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using ProductService.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. КОНФИГУРАЦИЯ (CONFIGURATION)
// ==========================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ==========================================
// 2. БАЗА ДАННЫХ (DATABASE)
// ==========================================

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(connectionString));

// ==========================================
// 3. ВНЕДРЕНИЕ ЗАВИСИМОСТЕЙ (DEPENDENCY INJECTION)
// ==========================================

// Репозитории
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Сервисы
builder.Services.AddScoped<IProductService, ProductService.Application.Services.ProductService>();

// AutoMapper (для маппинга сущностей в DTO и обратно)
builder.Services.AddAutoMapper(typeof(Program));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Контроллеры
builder.Services.AddControllers();

// ==========================================
// 4. АУТЕНТИФИКАЦИЯ (JWT AUTH)
// ==========================================

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

// ==========================================
// 5. SWAGGER (с кнопкой "AUTHORIZE")
// ==========================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Добавляем кнопку "Authorize" (авторизация) в Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите токен в формате: Bearer eyJhbGciOiJIUzI1Ni..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ==========================================
// 6. CORS (можно настроить для микросервисов)
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ==========================================
// Миграции базы данных
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ProductDbContext>();
        context.Database.Migrate();
        Console.WriteLine("База данных обновлена.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Произошла ошибка при попытке обновить базу данных.");
    }
}

// ==========================================
// 7. PIPELINE (обработка запросов)
// ==========================================

// Если мы в режиме разработки - показываем Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Отключаем HTTPS редирект в режиме разработки
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll"); // Включаем CORS

// Добавляем логирование запросов
app.Use(async (context, next) =>
{
    Console.WriteLine($"[ProductService] {context.Request.Method} {context.Request.Path}");
    await next();
});

// Добавляем логирование запросов
app.Use(async (context, next) =>
{
    Console.WriteLine($"[ProductService] {context.Request.Method} {context.Request.Path}");
    await next();
});

// Глобальный обработчик исключений
app.UseMiddleware<GlobalExceptionMiddleware>();

// Порядок: сначала аутентификация (кто ты?), потом авторизация (что ты можешь?)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Подключаем контроллеры

app.Run(); // Запускаем!

// Нужно для тестирования
public partial class Program { }
