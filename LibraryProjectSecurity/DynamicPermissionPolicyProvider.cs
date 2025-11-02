using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace LibraryProjectSecurity
{
    public class DynamicPermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public DynamicPermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

        public override Task<AuthorizationPolicy> GetPolicyAsync(string name)
        {
            if (name.StartsWith("PERM:"))
            {
                var key = name.Substring("PERM:".Length);
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(key))
                    .Build();
                return Task.FromResult(policy);
            }
            return base.GetPolicyAsync(name);
        }
    }
}
