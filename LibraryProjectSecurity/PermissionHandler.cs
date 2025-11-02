using LibraryProjectRepository.Repositories.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectSecurity
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly PermissionAtrRepository _perms;

        public PermissionHandler(PermissionAtrRepository perms) => _perms = perms;

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var idStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? context.User.FindFirst("sub")?.Value;

            if (Guid.TryParse(idStr, out var userId))
            {
                if (await _perms.UserHasAsync(userId, requirement.Key))
                    context.Succeed(requirement);
            }
        }
    }
}
