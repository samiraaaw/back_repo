using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Domain.Entities;
using DICREP.EcommerceSubastas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Infrastructure.Data.Repositories
{
    public class EmpleadoRepository : IEmpleadoRepository
    {
        private readonly EcoCircularContext _context;
        private readonly ILogger _logger;

        public EmpleadoRepository(EcoCircularContext context)
        {
            _context = context;
            _logger = Log.ForContext<EmpleadoRepository>(); // ✅ Serilog style
        }


        public async Task<Empleado> GetByIdAsync(int id)
        {
            return await _context.Empleados.FindAsync(id);
        }

        public async Task<Empleado> GetByUsuarioAsync(string usuario)
        {
            return await _context.Empleados.FirstOrDefaultAsync(r => r.EmpUsuario == usuario);
        }


        public async Task<IEnumerable<Empleado>> GetAllAsync()
        {
            return await _context.Empleados.ToListAsync();
        }

        public async Task<Empleado> CreateAsync(Empleado empleado)
        {
            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();
            return empleado;
        }

        public async Task<Empleado> UpdateAsync(Empleado empleado)
        {
            _context.Empleados.Update(empleado);
            await _context.SaveChangesAsync();
            return empleado;
        }

        public async Task<Empleado> DeleteAsync(Empleado empleado)
        {

            _context.Empleados.Remove(empleado);
            await _context.SaveChangesAsync();
            return empleado;
        }
    }

}
