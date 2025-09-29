using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICREP.EcommerceSubastas.Domain.Entities;

namespace DICREP.EcommerceSubastas.Application.Interfaces
{
    public interface IEmpleadoRepository
    {
        Task<Empleado> GetByIdAsync(int id);
        Task<Empleado> GetByUsuarioAsync(string usuario);
        Task<IEnumerable<Empleado>> GetAllAsync();
        Task<Empleado> CreateAsync(Empleado empleado);
        Task<Empleado> UpdateAsync(Empleado empleado);
        Task<Empleado> DeleteAsync(Empleado empleado);
    }
}
