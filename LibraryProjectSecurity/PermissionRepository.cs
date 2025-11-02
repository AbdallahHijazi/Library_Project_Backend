using LibraryProject;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectSecurity
{
    public class PermissionAtrRepository
    {
        private readonly LibraryDbContext context;
        private readonly IMemoryCache cache;

        public PermissionAtrRepository(LibraryDbContext context,
                                    IMemoryCache cache)
        {
            this.context = context;
            this.cache = cache;
        }
        public async Task<bool> UserHasAsync(Guid userId, string key, CancellationToken ct = default)
        {
            var cacheKey = $"perms:{userId}";
            if (!cache.TryGetValue<HashSet<string>>(cacheKey, out var set))
            {
                var rolePerms = await context.Users
                    .Where(u => u.Id == userId)
                    .SelectMany(u => u.Role.RolePermissions.Select(rp => rp.Permission.Key))
                    .ToListAsync(ct);

                var userPerms = await context.UserPermissions
                    .Where(up => up.UserId == userId)
                    .Select(up => up.Permission.Key)
                    .ToListAsync(ct);

                set = rolePerms.Concat(userPerms).ToHashSet(StringComparer.OrdinalIgnoreCase);
                cache.Set(cacheKey, set, TimeSpan.FromMinutes(10));
            }
            return set.Contains(key);
        }
    }

}
