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
    public class PerfilRepository : IPerfilRepository
    {
        private readonly EcoCircularContext _context;
        private readonly ILogger _logger;

        public PerfilRepository(EcoCircularContext context)
        {
            _context = context;
            _logger = Log.ForContext<PerfilRepository>(); // ✅ Serilog style
        }

        public async Task<Perfile> GetByIdAsync(int id)
        {
            return await _context.Perfiles.FindAsync(id);
        }

        public async Task<Perfile> GetByNombreAsync(string nombre)
        {
            return await _context.Perfiles.FirstOrDefaultAsync(r => r.PerfilNombre == nombre);
        }

        public async Task<IEnumerable<Perfile>> GetAllAsync()
        {
            return await _context.Perfiles.ToListAsync();
        }

        public async Task<Perfile> CreateAsync(Perfile perfil)
        {
            _context.Perfiles.Add(perfil);
            await _context.SaveChangesAsync();
            return perfil;
        }

        public async Task<Perfile> UpdateAsync(Perfile perfil)
        {
            _context.Perfiles.Update(perfil);
            await _context.SaveChangesAsync();
            return perfil;
        }

        public async Task<Perfile> DeleteAsync(Perfile perfil)
        {

            _context.Perfiles.Remove(perfil);
            await _context.SaveChangesAsync();
            return perfil;
        }


    }
}
