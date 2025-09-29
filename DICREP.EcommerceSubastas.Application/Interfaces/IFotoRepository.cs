using DICREP.EcommerceSubastas.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.Interfaces
{
    public interface IFotoRepository
    {
        Task<Foto> GetByIdAsync(int id);
        Task<Foto> GetByUrlAsync(string fotoUrl);
        Task<IEnumerable<Foto>> GetAllAsync();
        Task<Foto> CreateAsync(Foto foto);
        Task<Foto> UpdateAsync(Foto foto);
        Task<Foto> DeleteAsync(Foto foto);
        Task<IEnumerable<Foto>> GetByPrendaIdAsync(long prendaId);
        Task<IEnumerable<Foto>> GetByPrendaCLIdAsync(long clprendaId);
        Task<Foto> GetByUrlNormalizadoAsync(string url);

    }
}

