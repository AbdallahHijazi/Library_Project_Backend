using LibraryProject;
using LibraryProjectDomain.Models.PermissionModel;
using LibraryProjectDomain.Models.RoleModel;
using LibraryProjectRepository.SheardRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectRepository.Repositories.Roles
{
    public class RoleRepository : GinericRepository<Role>
    {
        public RoleRepository(LibraryDbContext context) : base(context)
        {
        }
    }
}
