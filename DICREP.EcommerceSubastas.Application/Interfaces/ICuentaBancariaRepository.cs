using DICREP.EcommerceSubastas.Application.DTOs.CLPrenda;
using DICREP.EcommerceSubastas.Application.DTOs.CuentaBancaria;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;

namespace DICREP.EcommerceSubastas.Application.Interfaces
{
    public interface ICuentaBancariaRepository
    {
        Task<ResponseDTO<CuentaBancariaResponseDTO>> GetOrCreateCuentaAsync(CuentaBancariaRequestDTO request);
    }
}