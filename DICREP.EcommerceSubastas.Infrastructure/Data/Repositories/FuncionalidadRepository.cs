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
    public class FuncionalidadRepository : IFuncionalidadRepository
    {
        private readonly EcoCircularContext _context;
        private readonly ILogger _logger;

        public FuncionalidadRepository(EcoCircularContext context)
        {
            _context = context;
            _logger = Log.ForContext<FuncionalidadRepository>(); // ✅ Serilog style
        }

        public async Task<IEnumerable<Funcionalidade>> GetAllAsync()
        {
            return await _context.Funcionalidades.ToListAsync();
        }
    }
}
