// DICREP.EcommerceSubastas.Application/Interfaces/IAuthClaveUnicaService.cs
using DICREP.EcommerceSubastas.Domain.Entities;

namespace DICREP.EcommerceSubastas.Application.Interfaces
{
    public interface IAuthClaveUnicaService
    {
        Task<ClaveUnicaTokenResponse?> ExchangeCodeForTokenAsync(string code, string state);
        Task<ClaveUnicaUserInfo?> GetUserInfoFromClaveUnicaAsync(string accessToken);
        Task<Empleado?> FindOrCreateUserFromClaveUnicaAsync(ClaveUnicaUserInfo userInfo);
        Task UpdateLastLoginAsync(int empId);
    }
}

// DTOs para Clave Única
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

public class LoginResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class UserDto
{
    public int EmpId { get; set; }
    public string EmpUsuario { get; set; } = string.Empty;
    public string EmpNombre { get; set; } = string.Empty;
    public string EmpApellido { get; set; } = string.Empty;
    public string EmpCorreo { get; set; } = string.Empty;
    public string? PerfilNombre { get; set; }
    public string? SucursalNombre { get; set; }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}