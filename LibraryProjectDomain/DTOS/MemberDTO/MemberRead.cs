using LibraryProjectDomain.DTOS.BorrowingDTO;
using LibraryProjectDomain.Models.BorrowingModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectDomain.DTOS.MemberDTO
{
    public class MemberRead
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public ICollection<BorrowingRead> Borrowings { get; set; }
        public int BorrowingsCount { get; set; }
    }
}
