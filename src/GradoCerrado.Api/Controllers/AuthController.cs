using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GradoCerrado.Domain.Models;
using System.Security.Cryptography;
using System.Text;

namespace GradoCerrado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly GradocerradoContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(GradocerradoContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] AuthRegisterRequest request)
    {
        try
        {
            // Validar datos
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { success = false, message = "Nombre y email son obligatorios" });
            }

            // Verificar si el email ya existe
            var existingUser = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Email.ToLower() == request.Email.ToLower());

            if (existingUser != null)
            {
                return BadRequest(new { success = false, message = "El email ya está registrado" });
            }

            // Crear nuevo estudiante
            var estudiante = new Estudiante
            {
                Nombre = request.Name.Trim(),
                Email = request.Email.ToLower().Trim(),
                PasswordHash = HashPassword(request.Password), // Usar la contraseña del request
                FechaRegistro = DateTime.UtcNow,
                UltimoAcceso = DateTime.UtcNow,
                Activo = true,
                Verificado = false
            };

            _context.Estudiantes.Add(estudiante);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario registrado: {Email}", request.Email);

            return Ok(new
            {
                success = true,
                message = "Usuario registrado exitosamente",
                user = new
                {
                    id = estudiante.Id,
                    name = estudiante.Nombre,
                    email = estudiante.Email
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registrando usuario: {Email}", request.Email);
            return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        }
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] AuthLoginRequest request)
    {
        try
        {
            // Validar datos
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { success = false, message = "Email y contraseña son obligatorios" });
            }

            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => string.Equals(e.Email, request.Email, StringComparison.OrdinalIgnoreCase) &&
                                         e.Activo == true);

            if (estudiante == null)
            {
                return BadRequest(new { success = false, message = "Credenciales incorrectas" });
            }

            // Verificar contraseña (por ahora solo verificamos que sea "123456")
            if (!VerifyPassword(request.Password, estudiante.PasswordHash))
            {
                return BadRequest(new { success = false, message = "Credenciales incorrectas" });
            }

            // Actualizar último acceso
            estudiante.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Login exitoso: {Email}", request.Email);

            return Ok(new
            {
                success = true,
                message = "Login exitoso",
                user = new
                {
                    id = estudiante.Id,
                    name = estudiante.Nombre,
                    email = estudiante.Email,
                    fechaRegistro = estudiante.FechaRegistro
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en login: {Email}", request.Email);
            return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        }
    }

    // Métodos auxiliares para hash de contraseñas
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashedPassword = HashPassword(password);
        return hashedPassword == hash;
    }
}

// DTOs para AuthController
public class AuthRegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // Agregar este campo
}

public class AuthLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}