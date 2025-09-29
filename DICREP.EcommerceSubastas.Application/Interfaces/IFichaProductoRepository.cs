using DICREP.EcommerceSubastas.Application.DTOs.FichaProducto;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.Interfaces
{
    public interface IFichaProductoRepository
    {
        Task<ResponseDTO<int>> SendFichaAsync(ReceiveFichaDto dto);
    }
}
