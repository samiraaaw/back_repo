using DICREP.EcommerceSubastas.Application.DTOs.Auth;
using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Domain.Entities;
using DICREP.EcommerceSubastas.Infrastructure.Configurations;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DICREP.EcommerceSubastas.Infrastructure.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger _logger;


        public JwtTokenGenerator(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
            _logger = Log.ForContext<JwtTokenGenerator>();
        }

        public JwtTokenResult GenerateToken(Empleado empleado)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, empleado.EmpUsuario),
                new Claim("perfilId", empleado.PerfilId.ToString()),
                new Claim("sucursalId", empleado.SucursalId.ToString()),
                new Claim("nombre", $"{empleado.EmpNombre} {empleado.EmpApellido}")
            };
            if (empleado.PerfilId == 3)
            {
                claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
                claims.Add(new Claim("EmpleadoID", empleado.EmpId.ToString()));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "Empleado"));
                claims.Add(new Claim("EmpleadoID", empleado.EmpId.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new JwtTokenResult
            {
                Token = tokenString,
                Expiration = expiration
            };
        }
    }
}
