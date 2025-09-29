using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DICREP.EcommerceSubastas.Application.Interfaces;

namespace DICREP.EcommerceSubastas.Infrastructure.Authorization
{

    public class PermisoHandler : AuthorizationHandler<PermisoRequirement>
    {
        private readonly IPermisoService _permisoService;

        public PermisoHandler(IPermisoService permisoService)
        {
            _permisoService = permisoService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermisoRequirement requirement)
        {
            var empleadoIdStr = context.User.FindFirst("EmpleadoID")?.Value;

            if (empleadoIdStr == null)
                return;

            var empleadoId = int.Parse(empleadoIdStr);

            if (await _permisoService.EmpleadoTienePermisoAsync(empleadoId, requirement.Codigo))
            {
                context.Succeed(requirement);
            }
        }
    }

}
