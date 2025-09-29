using DICREP.EcommerceSubastas.Application.DTOs.Auth;
using DICREP.EcommerceSubastas.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.Mappers
{
    public interface IFuncionalidadMapper
    {
        Funcionalidade ToEntity(FuncionalidadDTO dto);
        FuncionalidadDTO ToDTO(Funcionalidade entity);
        void UpdateEntity(Funcionalidade entity, FuncionalidadDTO dto);

    }

    public class FuncionalidadMapper : IFuncionalidadMapper
    {
        public Funcionalidade ToEntity(FuncionalidadDTO dto)
        {
            return new Funcionalidade
            {
                Nombre = dto.Nombre,
                EndpointApi = dto.EndpointApi,
                MetodoHttp = dto.MetodoHttp,
                Grupo = dto.Grupo,
                EsMenu = dto.EsMenu,
                Activo = dto.Activo,
                Codigo = dto.Codigo,
            };
        }


        public FuncionalidadDTO ToDTO(Funcionalidade entity)
        {
            return new FuncionalidadDTO
            {
                Nombre = entity.Nombre,
                EndpointApi = entity.EndpointApi,
                MetodoHttp = entity.MetodoHttp,
                Grupo = entity.Grupo,
                EsMenu = entity.EsMenu,
                Activo = entity.Activo,
                Codigo = entity.Codigo,
            };
        }

        public void UpdateEntity(Funcionalidade entity, FuncionalidadDTO dto)
        {
            entity.Nombre = dto.Nombre;
            entity.EndpointApi = dto.EndpointApi;
            entity.MetodoHttp = dto.MetodoHttp;
            entity.Grupo = dto.Grupo;
            entity.EsMenu = dto.EsMenu;
            entity.Activo = dto.Activo;
            entity.Codigo = dto.Codigo;
        }
    }
}
