// Controllers/AuthController.cs
using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IAuthClaveUnicaService _authClaveUnicaService;
    private readonly HttpClient _httpClient;
    private readonly EcoCircularContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthController(
        IConfiguration configuration,
        IAuthClaveUnicaService authClaveUnicaService,
        HttpClient httpClient,
        EcoCircularContext context,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _configuration = configuration;
        _authClaveUnicaService = authClaveUnicaService;
        _httpClient = httpClient;
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    [HttpPost("clave-unica")]
    public async Task<IActionResult> LoginClaveUnica([FromBody] ClaveUnicaLoginRequest request)
    {
        try
        {
            // 1. Intercambiar código por token
            var tokenResponse = await ExchangeCodeForToken(request.Code, request.State);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                return BadRequest(new { success = false, message = "Error al obtener token de Clave Única" });
            }

            // 2. Obtener información del usuario desde Clave Única
            var userInfo = await GetUserInfoFromClaveUnica(tokenResponse.AccessToken);

            if (userInfo == null)
            {
                return BadRequest(new { success = false, message = "Error al obtener información del usuario" });
            }

            // 3. Buscar o crear usuario en tu sistema
            var usuario = await _authService.FindOrCreateUserFromClaveUnica(userInfo);

            if (usuario == null || !usuario.EmpActivo)
            {
                return Unauthorized(new { success = false, message = "Usuario no autorizado o inactivo" });
            }

            // 4. Generar tokens JWT para tu sistema
            var jwtTokens = await _authService.GenerateTokens(usuario);

            // 5. Actualizar último acceso
            await _authService.UpdateLastLogin(usuario.EmpId);

            return Ok(new LoginResponse
            {
                Success = true,
                Token = jwtTokens.AccessToken,
                RefreshToken = jwtTokens.RefreshToken,
                User = new UserDto
                {
                    EmpId = usuario.EmpId,
                    EmpUsuario = usuario.EmpUsuario,
                    EmpNombre = usuario.EmpNombre,
                    EmpApellido = usuario.EmpApellido,
                    EmpCorreo = usuario.EmpCorreo,
                    PerfilNombre = usuario.Perfil?.PerfilNombre,
                    SucursalNombre = usuario.Sucursal?.SucursalNombre
                },
                Message = "Login exitoso con Clave Única"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor", error = ex.Message });
        }
    }

    private async Task<ClaveUnicaTokenResponse?> ExchangeCodeForToken(string code, string state)
    {
        var clientId = _configuration["ClaveUnica:ClientId"];
        var clientSecret = _configuration["ClaveUnica:ClientSecret"];
        var redirectUri = _configuration["ClaveUnica:RedirectUri"];

        var tokenEndpoint = "https://accounts.claveunica.gob.cl/openid/token";

        var requestData = new Dictionary<string, string>
        {
            {"grant_type", "authorization_code"},
            {"client_id", clientId},
            {"client_secret", clientSecret},
            {"code", code},
            {"redirect_uri", redirectUri}
        };

        var requestContent = new FormUrlEncodedContent(requestData);

        var response = await _httpClient.PostAsync(tokenEndpoint, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error exchanging code for token: {errorContent}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ClaveUnicaTokenResponse>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
    }

    private async Task<ClaveUnicaUserInfo?> GetUserInfoFromClaveUnica(string accessToken)
    {
        var userInfoEndpoint = "https://accounts.claveunica.gob.cl/openid/userinfo";

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.GetAsync(userInfoEndpoint);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error getting user info from Clave Única");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ClaveUnicaUserInfo>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Obtener el token del header
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                // Agregar token a blacklist o marcarlo como inválido
                await _authService.InvalidateToken(token);
            }

            return Ok(new { success = true, message = "Logout exitoso" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error durante logout", error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RefreshToken(request.RefreshToken);

            if (result == null)
            {
                return Unauthorized(new { success = false, message = "Refresh token inválido" });
            }

            return Ok(new { success = true, token = result.AccessToken, refreshToken = result.RefreshToken });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error al refrescar token", error = ex.Message });
        }
    }

    [HttpGet("validate-token")]
    [Authorize]
    public IActionResult ValidateToken()
    {
        return Ok(new { valid = true });
    }
}

// DTOs
public class ClaveUnicaLoginRequest
{
    public string Code { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}

public class ClaveUnicaTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}

public class ClaveUnicaUserInfo
{
    public string Sub { get; set; } = string.Empty; // RUT sin puntos ni guión
    public string Name { get; set; } = string.Empty;
    public ClaveUnicaName NameInfo { get; set; } = new();
    public string Email { get; set; } = string.Empty;
}

public class ClaveUnicaName
{
    public List<string> Given { get; set; } = new();
    public List<string> Family { get; set; } = new();
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}