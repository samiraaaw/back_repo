using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DICREP.EcommerceSubastas.Infrastructure.Authorization
{
    public class PermisoService : IPermisoService
    {
        private readonly EcoCircularContext _context;

        public PermisoService(EcoCircularContext context)
        {
            _context = context;
        }

        public async Task<bool> EmpleadoTienePermisoAsync(int empleadoId, string codigoFuncionalidad)
        {
            return await _context.Empleados
                .Where(e => e.EmpId == empleadoId)
                .SelectMany(e => e.Perfil.Permisos)
                .AnyAsync(p => p.Funcionalidad.Codigo == codigoFuncionalidad && p.Funcionalidad.Activo);
        }
    }

}
