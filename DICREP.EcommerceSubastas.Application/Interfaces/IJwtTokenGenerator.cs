using DICREP.EcommerceSubastas.Application.DTOs.Auth;
using DICREP.EcommerceSubastas.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        JwtTokenResult GenerateToken(Empleado empleado);
    }
}
