using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.Interfaces
{
    public interface IPermisoService
    {
        Task<bool> EmpleadoTienePermisoAsync(int empleadoId, string codigoFuncionalidad);
    }

}
