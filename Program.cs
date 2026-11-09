using EnglishSimulator.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;
using EnglishSimulator.Services;
using EnglishSimulator.Models.Database;

var builder = WebApplication.CreateBuilder(args);

// Servicios MVC
builder.Services.AddControllersWithViews();

// Obtener configuración de MySQL
var baseConnection = builder.Configuration
    .GetConnectionString("DefaultConnection");

var password = builder.Configuration[
    "ConnectionStrings:DbPassword"];

if (string.IsNullOrWhiteSpace(baseConnection))
{
    throw new InvalidOperationException(
        "Falta configurar la conexión a MySQL.");
}

// Si la contraseña es null, la convertimos a texto vacío para que MySQL la acepte
password ??= "";

// Construir cadena de conexión
var connectionBuilder = new MySqlConnectionStringBuilder(
    baseConnection)
{
    Password = password
};

// Registrar Entity Framework Core
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseMySql(
        connectionBuilder.ConnectionString,
        ServerVersion.Parse("8.4.10-mysql")
    )
);

builder.Services.AddScoped<GradingService>();
builder.Services.AddScoped<ExamService>();
builder.Services.AddScoped<ExamAttemptService>();
builder.Services.AddScoped<ExamExecutionService>();


builder.Services.AddScoped<
    IPasswordHasher<Usuario>,
    PasswordHasher<Usuario>>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "EnglishSimulator.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;

        options.Cookie.SecurePolicy =
            builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/dev/exam-preview/{type}",
        async (string type, ExamService examService) =>
        {
            var questions = await examService.GenerateExamAsync(type);

            return Results.Ok(new
            {
                ExamType = type,
                TotalQuestions = questions.Count,
                UniqueQuestions = questions
                    .Select(q => q.Id)
                    .Distinct()
                    .Count(),
                QuestionIds = questions
                    .Select(q => q.Id)
                    .ToList(),
                FirstQuestion = questions.FirstOrDefault()
            });
        });
}


// Manejo de errores
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


// Controladores de API con rutas por atributos
app.MapControllers();

// Ruta principal MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Index}/{id?}");

app.Run();
