using LibraryProjectDomain.Models.RoleModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectDomain.Models.PermissionModel
{
    public class Permission
    {
        public Permission()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Key { get; set; } = default!;
        public string Name { get; set; } = string.Empty;
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}
