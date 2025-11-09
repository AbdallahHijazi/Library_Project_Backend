using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectDomain.Models.UserModel
{
    public class PendingUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; 
        public string PasswordHash { get; set; } = string.Empty; 
        public string VerificationCode { get; set; } = string.Empty;
        public string LinkedEmail { get; set; } = string.Empty;
    }

}
