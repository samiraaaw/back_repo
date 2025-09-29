using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DICREP.EcommerceSubastas.Infrastructure.Authorization
{

    public class PermisoRequirement : IAuthorizationRequirement
    {
        public string Codigo { get; }

        public PermisoRequirement(string codigo)
        {
            Codigo = codigo;
        }
    }

}
