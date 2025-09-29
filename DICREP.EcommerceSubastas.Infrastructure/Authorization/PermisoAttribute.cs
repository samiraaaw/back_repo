using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


//Este atributo será un decorador para usar en los controladores o acciones
//y que internamente use Authorize con la política correcta:
namespace DICREP.EcommerceSubastas.Infrastructure.Authorization
{
    public class PermisoAttribute : AuthorizeAttribute
    {
        public PermisoAttribute(string codigoFuncionalidad)
        {
            Policy = $"Permiso:{codigoFuncionalidad}";
        }
    }
}
