using DICREP.EcommerceSubastas.Application.DTOs.FichaProducto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.Interfaces
{
    public interface IClAuctionApiService
    {
        Task<ClApiResponse> UpdateProductStatusAsync(string productId, int statusId);
    }
}
