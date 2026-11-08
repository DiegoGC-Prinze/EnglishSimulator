using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using EnglishSimulator.Data;
using EnglishSimulator.Models.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Microsoft.AspNetCore.Antiforgery;

namespace EnglishSimulator.Controllers;

public record RegisterRequest(
    string Name,
    string Email,
    string Password);

public record LoginRequest(
    string Email,
    string Password);

[ApiController]
[AutoValidateAntiforgeryToken]
[Route("api/auth")]
public class ApiAuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public ApiAuthController(
        ApplicationDbContext context,
        IPasswordHasher<Usuario> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("register")]

[HttpGet("csrf")]
[AllowAnonymous]
public IActionResult GetCsrfToken(
    [FromServices] IAntiforgery antiforgery)
{
    var tokens = antiforgery.GetAndStoreTokens(HttpContext);

    Response.Headers["Cache-Control"] = "no-store";

    return Ok(new
    {
        token = tokens.RequestToken
    });
}


    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        string name = request.Name?.Trim() ?? "";
        string email = request.Email?.Trim().ToLowerInvariant() ?? "";
        string password = request.Password ?? "";

        if (name.Length is < 2 or > 100 ||
            email.Length > 100 ||
            !new EmailAddressAttribute().IsValid(email) ||
            password.Length < 8)
        {
            return BadRequest(new
            {
                message = "Nombre, correo o contraseña inválidos. " +
                          "La contraseña debe tener al menos 8 caracteres."
            });
        }

        bool exists = await _context.Set<Usuario>()
            .AnyAsync(u => u.Correo == email);

        if (exists)
            return Conflict(new
            {
                message = "Este correo ya está registrado."
            });

        var user = new Usuario
        {
            Nombre = name,
            Correo = email,
            FechaRegistro = DateTime.UtcNow
        };

        // Nunca almacenar contraseñas en texto plano.
        user.Contrasena = _passwordHasher.HashPassword(
            user, password);

        _context.Set<Usuario>().Add(user);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is MySqlException { Number: 1062 })
        {
            return Conflict(new
            {
                message = "Este correo ya está registrado."
            });
        }

        await SignInUserAsync(user);

        return Ok(new
        {
            message = "Usuario registrado correctamente.",
            user = new
            {
                id = user.IdUsuario,
                name = user.Nombre,
                email = user.Correo
            }
        });
    }


[HttpPost("login")]
[AllowAnonymous]
public async Task<IActionResult> Login(
    [FromBody] LoginRequest request)
{
    string email = request.Email?.Trim().ToLowerInvariant() ?? "";
    string password = request.Password ?? "";

    var user = await _context.Set<Usuario>()
        .SingleOrDefaultAsync(u => u.Correo == email);

    if (user is null)
        return Unauthorized(new
        {
            message = "Credenciales incorrectas."
        });

    var verification = _passwordHasher.VerifyHashedPassword(
        user,
        user.Contrasena,
        password);

    if (verification == PasswordVerificationResult.Failed)
        return Unauthorized(new
        {
            message = "Credenciales incorrectas."
        });

    if (verification ==
        PasswordVerificationResult.SuccessRehashNeeded)
    {
        user.Contrasena = _passwordHasher.HashPassword(
            user, password);

        await _context.SaveChangesAsync();
    }

    await SignInUserAsync(user);

    return Ok(new
    {
        message = "Inicio de sesión exitoso.",
        user = new
        {
            id = user.IdUsuario,
            name = user.Nombre,
            email = user.Correo
        }
    });
}


    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        string? id = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!uint.TryParse(id, out uint userId))
            return Unauthorized();

        var user = await _context.Set<Usuario>()
            .AsNoTracking()
            .Where(u => u.IdUsuario == userId)
            .Select(u => new
            {
                id = u.IdUsuario,
                name = u.Nombre,
                email = u.Correo
            })
            .SingleOrDefaultAsync();

        return user is null ? Unauthorized() : Ok(user);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Ok(new
        {
            message = "Sesión cerrada."
        });
    }

    private async Task SignInUserAsync(Usuario user)
    {
        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, user.Nombre)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });
    }
}
