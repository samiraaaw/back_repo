using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.Interfaces
{
    public interface IFeriadosRepository
    {
        Task<bool> ExistsByFechaAsync(DateOnly fecha);
        Task<ResponseDTO<int>> populateHolidaysTableBulk(List<Feriado> listFeriados);
    }
}
