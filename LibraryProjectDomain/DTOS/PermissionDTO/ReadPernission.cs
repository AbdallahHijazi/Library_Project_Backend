using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectDomain.DTOS.PermissionDTO
{
    public class ReadPernission
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = default!;
        public string? Name { get; set; }
    }
}
