using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DICREP.EcommerceSubastas.Infrastructure.Authorization
{

    public class PermisoPolicyProvider : IAuthorizationPolicyProvider
    {
        const string PREFIX = "Permiso:";

        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
        private readonly IServiceProvider _serviceProvider;

        public PermisoPolicyProvider(IOptions<AuthorizationOptions> options, IServiceProvider serviceProvider)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
            _serviceProvider = serviceProvider;
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

        public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(PREFIX))
            {
                var codigo = policyName.Substring(PREFIX.Length);

                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermisoRequirement(codigo))
                    .Build();

                return await Task.FromResult(policy);
            }

            return await _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }

}
