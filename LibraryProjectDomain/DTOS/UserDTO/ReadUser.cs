using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectDomain.DTOS.UserDTO
{
    public class ReadUser
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public Guid? RoleId { get; set; }
        public List<Guid>? PermissionIds { get; set; }
    }
}
