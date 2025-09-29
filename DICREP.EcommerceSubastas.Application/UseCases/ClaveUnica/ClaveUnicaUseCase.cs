using DICREP.EcommerceSubastas.Application.DTOs.CuentaBancaria;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Application.UseCases.CuentaBancaria;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.UseCases.ClaveUnica
{
    public class ClaveUnicaUseCase
    {
        private readonly ILogger _logger;
        private readonly IAuthClaveUnicaService _authClaveUnicaService;

        public ClaveUnicaUseCase()
        {
            _logger = Log.ForContext<ClaveUnicaUseCase>();
        }

        public async Task<ResponseDTO<CuentaBancariaResponseDTO>> ExecuteAsync(ClaveUnicaLoginRequest request)
        {
            // 3. Buscar o crear usuario en tu sistema
            var usuario = await _authClaveUnicaService.FindOrCreateUserFromClaveUnica(userInfo);

            if (usuario == null || !usuario.EmpActivo)
            {
                return Unauthorized(new { success = false, message = "Usuario no autorizado o inactivo" });
            }

// 4. Generar tokens JWT para tu sistema
var jwtTokens = await _authClaveUnicaService.GenerateTokens(usuario);

// 5. Actualizar último acceso
await _authClaveUnicaService.UpdateLastLogin(usuario.EmpId);

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
}
