// DICREP.EcommerceSubastas.Infrastructure/Services/AuthClaveUnicaService.cs
using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Domain.Entities;
using DICREP.EcommerceSubastas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DICREP.EcommerceSubastas.Infrastructure.Services
{
    public class AuthClaveUnicaService : IAuthClaveUnicaService
    {
        private readonly EcoCircularContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public AuthClaveUnicaService(
            EcoCircularContext context,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = Log.ForContext<AuthClaveUnicaService>();
        }

        public async Task<ClaveUnicaTokenResponse?> ExchangeCodeForTokenAsync(string code, string state)
        {
            try
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

                _logger.Information("Intercambiando código por token con Clave Única");
                var response = await _httpClient.PostAsync(tokenEndpoint, requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.Error("Error al intercambiar código: {StatusCode} - {Content}",
                        response.StatusCode, errorContent);
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ClaveUnicaTokenResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inesperado al intercambiar código por token");
                return null;
            }
        }

        public async Task<ClaveUnicaUserInfo?> GetUserInfoFromClaveUnicaAsync(string accessToken)
        {
            try
            {
                var userInfoEndpoint = "https://accounts.claveunica.gob.cl/openid/userinfo";

                using var request = new HttpRequestMessage(HttpMethod.Get, userInfoEndpoint);
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                _logger.Information("Obteniendo información del usuario desde Clave Única");
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.Error("Error al obtener info del usuario: {StatusCode} - {Content}",
                        response.StatusCode, errorContent);
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ClaveUnicaUserInfo>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inesperado al obtener información del usuario");
                return null;
            }
        }

        public async Task<Empleado?> FindOrCreateUserFromClaveUnicaAsync(ClaveUnicaUserInfo userInfo)
        {
            try
            {
                // Formatear RUT
                var rutSinFormato = userInfo.Sub;
                var rutFormateado = FormatearRut(rutSinFormato);
                var rutNumerico = ExtraerRutNumerico(rutSinFormato);
                var digitoVerificador = ExtraerDigitoVerificador(rutSinFormato);

                _logger.Information("Buscando usuario con RUT: {RutFormateado} (numérico: {RutNumerico})",
                    rutFormateado, rutNumerico);

                // Buscar usuario existente por RUT numérico
                var usuario = await _context.Empleados
                    .Include(e => e.Perfil)
                    .Include(e => e.Sucursal)
                    .FirstOrDefaultAsync(e => e.EmpRut == rutNumerico);

                if (usuario != null)
                {
                    _logger.Information("Usuario encontrado: {EmpId} - {EmpUsuario}",
                        usuario.EmpId, usuario.EmpUsuario);

                    // Actualizar información si es necesario
                    await ActualizarInformacionUsuarioAsync(usuario, userInfo);
                    return usuario;
                }

                // Usuario no existe - verificar si permitimos creación automática
                var permitirCreacion = _configuration.GetValue<bool>("ClaveUnica:AllowAutoUserCreation", false);

                if (!permitirCreacion)
                {
                    _logger.Warning("Usuario con RUT {RutFormateado} no encontrado y creación automática deshabilitada",
                        rutFormateado);
                    return null;
                }

                // Crear nuevo usuario
                return await CrearNuevoUsuarioAsync(userInfo, rutNumerico, digitoVerificador);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error al buscar o crear usuario desde Clave Única");
                return null;
            }
        }

        private async Task<Empleado> CrearNuevoUsuarioAsync(ClaveUnicaUserInfo userInfo, int rutNumerico, string digitoVerificador)
        {
            _logger.Information("Creando nuevo usuario desde Clave Única");

            // Obtener perfil y sucursal por defecto
            var perfilDefecto = await _context.Perfiles
                .FirstOrDefaultAsync(p => p.PerfilNombre.Contains("Usuario") || p.PerfilId == 1);

            var sucursalDefecto = await _context.Sucursales
                .FirstOrDefaultAsync(s => s.SucursalNombre.Contains("Central") || s.SucursalId == 1);

            var nuevoUsuario = new Empleado
            {
                EmpRut = rutNumerico,
                EmpRutDig = digitoVerificador,
                EmpNombre = NormalizarNombre(userInfo.NameInfo.Given.FirstOrDefault() ?? "Sin nombre"),
                EmpApellido = NormalizarNombre(userInfo.NameInfo.Family.FirstOrDefault() ?? "Sin apellido"),
                EmpCorreo = userInfo.Email?.ToLower() ?? "",
                EmpUsuario = GenerarNombreUsuario(userInfo),
                EmpActivo = true,
                AuthMethod = 2, // Clave Única
                PerfilId = perfilDefecto?.PerfilId ?? 1,
                SucursalId = sucursalDefecto?.SucursalId ?? 1,
                ClaveUnicaSub = userInfo.Sub
            };

            _context.Empleados.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            _logger.Information("Usuario creado exitosamente: {EmpId} - {EmpUsuario}",
                nuevoUsuario.EmpId, nuevoUsuario.EmpUsuario);

            // Recargar con includes
            return await _context.Empleados
                .Include(e => e.Perfil)
                .Include(e => e.Sucursal)
                .FirstAsync(e => e.EmpId == nuevoUsuario.EmpId);
        }

        private async Task ActualizarInformacionUsuarioAsync(Empleado usuario, ClaveUnicaUserInfo userInfo)
        {
            var actualizado = false;

            // Actualizar ClaveUnicaSub si no existe
            if (string.IsNullOrEmpty(usuario.ClaveUnicaSub))
            {
                usuario.ClaveUnicaSub = userInfo.Sub;
                actualizado = true;
            }

            // Actualizar email si cambió
            if (!string.IsNullOrEmpty(userInfo.Email) &&
                !string.Equals(usuario.EmpCorreo, userInfo.Email, StringComparison.OrdinalIgnoreCase))
            {
                usuario.EmpCorreo = userInfo.Email.ToLower();
                actualizado = true;
            }

            if (actualizado)
            {
                await _context.SaveChangesAsync();
                _logger.Information("Información del usuario {EmpId} actualizada", usuario.EmpId);
            }
        }

        public async Task UpdateLastLoginAsync(int empId)
        {
            try
            {
                var usuario = await _context.Empleados.FindAsync(empId);
                if (usuario != null)
                {
                    usuario.EmpFechaLog = DateTime.Now;
                    await _context.SaveChangesAsync();
                    _logger.Information("Último login actualizado para usuario {EmpId}", empId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error al actualizar último login para usuario {EmpId}", empId);
            }
        }

        // Métodos auxiliares
        private string FormatearRut(string rutSinFormato)
        {
            if (string.IsNullOrEmpty(rutSinFormato) || rutSinFormato.Length < 2)
                return rutSinFormato;

            var numero = rutSinFormato.Substring(0, rutSinFormato.Length - 1);
            var dv = rutSinFormato.Substring(rutSinFormato.Length - 1);

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

        private int ExtraerRutNumerico(string rutSinFormato)
        {
            if (string.IsNullOrEmpty(rutSinFormato) || rutSinFormato.Length < 2)
                return 0;

            var numero = rutSinFormato.Substring(0, rutSinFormato.Length - 1);
            return int.TryParse(numero, out var result) ? result : 0;
        }

        private string ExtraerDigitoVerificador(string rutSinFormato)
        {
            if (string.IsNullOrEmpty(rutSinFormato) || rutSinFormato.Length < 1)
                return "0";

            return rutSinFormato.Substring(rutSinFormato.Length - 1).ToUpper();
        }

        private string GenerarNombreUsuario(ClaveUnicaUserInfo userInfo)
        {
            var nombre = userInfo.NameInfo.Given.FirstOrDefault()?.ToLower() ?? "usuario";
            var apellido = userInfo.NameInfo.Family.FirstOrDefault()?.ToLower() ?? "";

            var usuario = $"{nombre.FirstOrDefault()}{apellido}";

            // Limpiar caracteres especiales
            usuario = Regex.Replace(usuario, @"[^a-zA-Z0-9]", "");

            return usuario.Length > 30 ? usuario.Substring(0, 30) : usuario;
        }

        private string NormalizarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return "";

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nombre.ToLower().Trim());
        }
    }
}