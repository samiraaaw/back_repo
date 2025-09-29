// Interfaces/ICLPrendaRepository.cs
using DICREP.EcommerceSubastas.Application.DTOs.CLPrenda;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;

namespace DICREP.EcommerceSubastas.Application.Interfaces
{
    public interface ICLPrendaRepository
    {
        Task<ResponseDTO<CLPrendaUpdateResponseDTO>> UpdateIncrementoComisionAsync(CLPrendaUpdateRequestDTO request);
    }
}
