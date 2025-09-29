// Services/IAuthService.cs
using DICREP.EcommerceSubastas.Domain.Entities;
using DICREP.EcommerceSubastas.Infrastructure.Persistence;
using Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public interface IAuthService
{
    Task<Empleado?> FindOrCreateUserFromClaveUnica(ClaveUnicaUserInfo userInfo);
    Task<TokenResult> GenerateTokens(Empleado usuario);
    Task<TokenResult?> RefreshToken(string refreshToken);
    Task UpdateLastLogin(int empId);
    Task InvalidateToken(string token);
}

// Services/AuthService.cs
public class AuthService : IAuthService
{
    private readonly EcoCircularContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(EcoCircularContext context, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Empleado?> FindOrCreateUserFromClaveUnica(ClaveUnicaUserInfo userInfo)
    {
        try
        {
            // Extraer RUT del sub (viene sin puntos ni guión)
            var rutSinFormato = userInfo.Sub;
            var rutFormateado = FormatearRut(rutSinFormato);

            // Buscar usuario existente por RUT
            var usuario = await _context.Empleados
                .Include(e => e.Perfil)
                .Include(e => e.Sucursal)
                .FirstOrDefaultAsync(e => e.EmpRut == rutFormateado || e.EmpRut == rutSinFormato);

            if (usuario != null)
            {
                // Usuario existe, actualizar información si es necesario
                await ActualizarInformacionUsuario(usuario, userInfo);
                return usuario;
            }

            // Usuario no existe, verificar si se permite creación automática
            var permitirCreacionAutomatica = _configuration.GetValue<bool>("ClaveUnica:AllowAutoUserCreation", false);

            if (!permitirCreacionAutomatica)
            {
                _logger.LogWarning($"Usuario con RUT {rutFormateado} no encontrado y creación automática deshabilitada");
                return null;
            }

            // Crear nuevo usuario
            return await CrearNuevoUsuario(userInfo, rutFormateado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar o crear usuario desde Clave Única");
            return null;
        }
    }

    private async Task<Empleado> CrearNuevoUsuario(ClaveUnicaUserInfo userInfo, string rutFormateado)
    {
        // Obtener perfil por defecto para nuevos usuarios
        var perfilDefecto = await _context.Perfiles
            .FirstOrDefaultAsync(p => p.PerfilNombre == "Usuario Básico" || p.PerfilId == 1);

        // Obtener sucursal por defecto
        var sucursalDefecto = await _context.Sucursales
            .FirstOrDefaultAsync(s => s.SucursalNombre.Contains("Central") || s.SucursalId == 1);

        var nuevoUsuario = new Empleado
        {
            EmpRut = rutFormateado,
            EmpNombre = userInfo.NameInfo.Given.FirstOrDefault() ?? "Sin nombre",
            EmpApellido = userInfo.NameInfo.Family.FirstOrDefault() ?? "Sin apellido",
            EmpCorreo = userInfo.Email,
            EmpUsuario = GenerarNombreUsuario(userInfo),
            EmpActivo = true,
            AuthMethod = 2, // Clave Única
            PerfilId = perfilDefecto?.PerfilId ?? 1,
            SucursalId = sucursalDefecto?.SucursalId ?? 1
        };

        _context.Empleados.Add(nuevoUsuario);
        await _context.SaveChangesAsync();

        // Recargar con includes
        return await _context.Empleados
            .Include(e => e.Perfil)
            .Include(e => e.Sucursal)
            .FirstAsync(e => e.EmpId == nuevoUsuario.EmpId);
    }

    private async Task ActualizarInformacionUsuario(Empleado usuario, ClaveUnicaUserInfo userInfo)
    {
        var actualizado = false;

        // Actualizar email si cambió
        if (!string.IsNullOrEmpty(userInfo.Email) && usuario.EmpCorreo != userInfo.Email)
        {
            usuario.EmpCorreo = userInfo.Email;
            actualizado = true;
        }

        // Actualizar nombres si están vacíos
        var primerNombre = userInfo.NameInfo.Given.FirstOrDefault();
        if (!string.IsNullOrEmpty(primerNombre) && string.IsNullOrEmpty(usuario.EmpNombre))
        {
            usuario.EmpNombre = primerNombre;
            actualizado = true;
        }

        var primerApellido = userInfo.NameInfo.Family.FirstOrDefault();
        if (!string.IsNullOrEmpty(primerApellido) && string.IsNullOrEmpty(usuario.EmpApellido))
        {
            usuario.EmpApellido = primerApellido;
            actualizado = true;
        }

        if (actualizado)
        {
            await _context.SaveChangesAsync();
        }
    }

    public async Task<TokenResult> GenerateTokens(Empleado usuario)
    {
        var jwtSecret = _configuration["Jwt:Secret"];
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];
        var jwtExpireMinutes = _configuration.GetValue<int>("Jwt:ExpireMinutes", 60);
        var refreshExpireDays = _configuration.GetValue<int>("Jwt:RefreshExpireDays", 7);

        // Crear claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.EmpId.ToString()),
            new(ClaimTypes.Name, usuario.EmpUsuario),
            new(ClaimTypes.Email, usuario.EmpCorreo ?? ""),
            new("empId", usuario.EmpId.ToString()),
            new("empNombre", usuario.EmpNombre),
            new("empApellido", usuario.EmpApellido),
            new("perfilId", usuario.PerfilId.ToString()),
            new("perfilNombre", usuario.Perfil?.PerfilNombre ?? ""),
            new("sucursalId", usuario.SucursalId.ToString()),
            new("sucursalNombre", usuario.Sucursal?.SucursalNombre ?? "")
        };

        // Generar access token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var accessToken = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(jwtExpireMinutes),
            signingCredentials: creds
        );

        // Generar refresh token
        var refreshToken = GenerateRefreshToken();

        // Guardar refresh token en base de datos
        await GuardarRefreshToken(usuario.EmpId, refreshToken, DateTime.Now.AddDays(refreshExpireDays));

        return new TokenResult
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.Now.AddMinutes(jwtExpireMinutes)
        };
    }

    public async Task<TokenResult?> RefreshToken(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.Empleado)
                .ThenInclude(e => e.Perfil)
            .Include(rt => rt.Empleado)
                .ThenInclude(e => e.Sucursal)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.ExpiresAt > DateTime.Now && !rt.IsRevoked);

        if (storedToken == null)
        {
            return null;
        }

        // Revocar el token usado
        storedToken.IsRevoked = true;
        await _context.SaveChangesAsync();

        // Generar nuevos tokens
        return await GenerateTokens(storedToken.Empleado);
    }

    public async Task UpdateLastLogin(int empId)
    {
        var usuario = await _context.Empleados.FindAsync(empId);
        if (usuario != null)
        {
            usuario.EmpFechaLog = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task InvalidateToken(string token)
    {
        // Implementar blacklist de tokens si es necesario
        // Por ahora, solo registrar el logout
        _logger.LogInformation($"Token invalidated: {token.Substring(0, 10)}...");
    }

    // Métodos auxiliares
    private string FormatearRut(string rutSinFormato)
    {
        if (string.IsNullOrEmpty(rutSinFormato) || rutSinFormato.Length < 2)
            return rutSinFormato;

        var numero = rutSinFormato.Substring(0, rutSinFormato.Length - 1);
        var dv = rutSinFormato.Substring(rutSinFormato.Length - 1);

        // Formatear con puntos
        if (numero.Length > 6)
        {
            return $"{numero.Substring(0, numero.Length - 6)}.{numero.Substring(numero.Length - 6, 3)}.{numero.Substring(numero.Length - 3)}-{dv}";
        }
        else if (numero.Length > 3)
        {
            return $"{numero.Substring(0, numero.Length - 3)}.{numero.Substring(numero.Length - 3)}-{dv}";
        }
        else
        {
            return $"{numero}-{dv}";
        }
    }

    private string GenerarNombreUsuario(ClaveUnicaUserInfo userInfo)
    {
        var nombre = userInfo.NameInfo.Given.FirstOrDefault()?.ToLower() ?? "usuario";
        var apellido = userInfo.NameInfo.Family.FirstOrDefault()?.ToLower() ?? "";

        var usuario = $"{nombre.FirstOrDefault()}{apellido}";

        // Limpiar caracteres especiales
        usuario = System.Text.RegularExpressions.Regex.Replace(usuario, @"[^a-zA-Z0-9]", "");

        return usuario.Length > 30 ? usuario.Substring(0, 30) : usuario;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task GuardarRefreshToken(int empId, string token, DateTime expiresAt)
    {
        var refreshToken = new RefreshToken
        {
            EmpId = empId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.Now,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }
}

// Modelos auxiliares
public class TokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class RefreshToken
{
    public int Id { get; set; }
    public int EmpId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
    public Empleado Empleado { get; set; } = null!;
}