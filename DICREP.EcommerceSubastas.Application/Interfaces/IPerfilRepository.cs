using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICREP.EcommerceSubastas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DICREP.EcommerceSubastas.Application.Interfaces
{
    public interface IPerfilRepository
    {
        Task<Perfile> GetByIdAsync(int id);
        Task<Perfile> GetByNombreAsync(string nombre);
        Task<IEnumerable<Perfile>> GetAllAsync();
        Task<Perfile> CreateAsync(Perfile perfil);
        Task<Perfile> UpdateAsync(Perfile perfil);
        Task<Perfile> DeleteAsync(Perfile perfil);
    }
}
