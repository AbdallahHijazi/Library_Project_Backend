using LibraryProject;
using LibraryProjectDomain.DTOS.PermissionDTO;
using LibraryProjectDomain.Models.PermissionModel;
using LibraryProjectRepository.SheardRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectRepository.Repositories.Permissions
{
    public class PermissionRepository : GinericRepository<Permission>
    {
        private readonly LibraryDbContext context;
        private readonly IMemoryCache cache;

        public PermissionRepository(LibraryDbContext context,
                                    IMemoryCache cache) : base(context)
        {
            this.context = context;
            this.cache = cache;
        }
        public async Task<List<Permission>> GetAllPermissions()
        {
            var permissions = await context.Permissions.ToListAsync();
            if (permissions == null)
                return null;
            return permissions;
        }
        public async Task<Permission> GetPermissionById(Guid id)
        {
            var permission = await context.Permissions.FirstOrDefaultAsync(p => p.Id == id);
            if (permission == null)
                return null;
            return permission;
        }
        public async Task<Permission> CreatePermission(PermissionOparation create)
        {
            if (string.IsNullOrWhiteSpace(create.Key))
                throw new ArgumentException("Permission key is required");

            if (await context.Permissions.AnyAsync(p => p.Key == create.Key))
                throw new InvalidOperationException("Permission key already exists");

            var newPermission = new Permission
            {
                Key = create.Key.Trim(),
                Name = create.Name,
            };

            context.Permissions.Add(newPermission);
            await context.SaveChangesAsync();
            return newPermission;
        }
        public async Task<Permission> UpdatePermission(Guid id,PermissionOparation update)
        {
            var permission = await context.Permissions.FirstOrDefaultAsync(p => p.Id == id);
            if (permission == null)
                return null;
            permission.Key = update.Key.Trim();
            permission.Name = update.Name;

            await context.SaveChangesAsync();
            return permission;
        }
        public async Task<bool> DeletePermission(Guid id)
        {
            var permission = await context.Permissions.FirstOrDefaultAsync(p => p.Id == id);
            if (permission == null)
                return false;
            context.Permissions.Remove(permission);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
