using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectDomain.DTOS.UserDTO
{
    public class RevokedToken
    {
        public int Id { get; set; }                     
        public Guid UserId { get; set; }                
        public string Jti { get; set; } = default!;    
        public DateTime RevokedAtUtc { get; set; }     
    }
}
