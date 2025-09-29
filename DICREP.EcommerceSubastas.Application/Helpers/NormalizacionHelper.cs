using System.Globalization;

namespace DICREP.EcommerceSubastas.Application.Helpers
{
    public static class NormalizacionHelper
    {
        // Uso de HashSet para mejorar rendimiento en búsquedas
        private static readonly HashSet<string> Minusculas = new(StringComparer.OrdinalIgnoreCase)
        {
            "de", "del", "la", "las", "los", "y", "a", "el"
        };

        public static string NormalizarNombre(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";
            var palabras = texto.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < palabras.Length; i++)
            {
                // Primera palabra siempre en mayúscula, y resto solo si no está en la lista de preposiciones/artículos
                if (i == 0 || !Minusculas.Contains(palabras[i]))
                {
                    palabras[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(palabras[i]);
                }
            }
            return string.Join(' ', palabras);
        }
    }
}

