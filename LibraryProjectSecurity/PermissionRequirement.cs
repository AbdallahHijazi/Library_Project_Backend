using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectSecurity
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Key { get; }
        public PermissionRequirement(string key) => Key = key;
    }
}
