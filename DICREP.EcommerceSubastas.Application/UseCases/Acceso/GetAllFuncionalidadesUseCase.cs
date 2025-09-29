using DICREP.EcommerceSubastas.Application.DTOs.Auth;
using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Application.Mappers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.UseCases.Acceso
{
    public class GetAllFuncionalidadesUseCase
    {
        private readonly IFuncionalidadRepository _ifuncionalidadRepository;
        private readonly IFuncionalidadMapper _ifuncionalidadMapper;
        private readonly ILogger _logger;


        public GetAllFuncionalidadesUseCase(IFuncionalidadRepository ifuncionalidadRepository, IFuncionalidadMapper ifuncionalidadMapper)
        {
            _ifuncionalidadRepository = ifuncionalidadRepository;
            _ifuncionalidadMapper = ifuncionalidadMapper;
            _logger = Log.ForContext<GetAllFuncionalidadesUseCase>();
        }

        public async Task<IEnumerable<FuncionalidadDTO>> ExecuteAsync()
        {
            try
            {
                var funcionalidades = await _ifuncionalidadRepository.GetAllAsync();
                if (funcionalidades == null)
                    return Enumerable.Empty<FuncionalidadDTO>();

                return funcionalidades.Select(r => _ifuncionalidadMapper.ToDTO(r));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error real al obtener las funcionalidades.");
                throw new Exception("Error al obtener las funcionalidades.");
            }
        }

    }
}


