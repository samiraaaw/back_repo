using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DICREP.EcommerceSubastas.Application.Validation
{
    public sealed class RutValidAttribute : ValidationAttribute
    {
        // Regex para validar formato XX.XXX.XXX-Y o X.XXX.XXX-Y
        private static readonly Regex RutRegex = new(@"^\d{1,2}(\.\d{3}){2}-[0-9K]$", RegexOptions.Compiled);

        // Secuencias inválidas (todos los dígitos iguales)
        private static readonly HashSet<string> SecuenciasInvalidas = new()
        {
            "00000000","11111111","22222222","33333333",
            "44444444","55555555","66666666","77777777",
            "88888888","99999999"
        };

        public override bool IsValid(object value)
        {
            if (value == null) return false;

            string rutOriginal = value.ToString().ToUpper().Trim();

            // Validar formato
            if (!RutRegex.IsMatch(rutOriginal)) return false;

            // Limpiar rut (quitar puntos y guion)
            string rutLimpio = rutOriginal.Replace(".", "").Replace("-", "");
            string cuerpo = rutLimpio[..^1];
            char dv = rutLimpio[^1];

            // Revisar secuencias inválidas
            if (SecuenciasInvalidas.Contains(cuerpo)) return false;

            // Validar dígito verificador
            return CalcularDV(cuerpo) == dv;
        }

        private static char CalcularDV(string cuerpo)
        {
            int suma = 0;
            int multiplicador = 2;

            for (int i = cuerpo.Length - 1; i >= 0; i--)
            {
                suma += (cuerpo[i] - '0') * multiplicador;
                multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
            }

            int resto = 11 - (suma % 11);
            return resto switch
            {
                11 => '0',
                10 => 'K',
                _ => resto.ToString()[0]
            };
        }
    }
}
