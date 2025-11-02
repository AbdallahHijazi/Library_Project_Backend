using LibraryProjectDomain.Models.PermissionModel;
using LibraryProjectDomain.Models.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectDomain.Models.RoleModel
{
    public class UserPermission
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = default!;
    }
}
